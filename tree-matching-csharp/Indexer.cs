using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Neighbors = System.Collections.Generic.Dictionary<tree_matching_csharp.Node, System.Collections.Generic.HashSet<tree_matching_csharp.Scored<tree_matching_csharp.Node>>>;

namespace tree_matching_csharp
{
    public class Indexer
    {
        private const    string        IndexName = "wehave-node";
        private readonly ElasticClient _client;

        public Indexer()
        {
            var settings = new ConnectionSettings()
                .DefaultIndex(IndexName)
//                .EnableDebugMode()
                .DefaultMappingFor<Node>(m => m
                    .IdProperty(n => n.Id)
                    .Ignore(n => n.Parent)
                    .Ignore(n => n.Signature)
                );
            _client = new ElasticClient(settings);
        }

        private async Task CreateIndexIfNotExist()
        {
            if (_client.Indices.Exists(IndexName).Exists)
                await _client.Indices.DeleteAsync(IndexName);

            await _client.Indices.CreateAsync(IndexName, c => c
                .Map<Node>(m => m
                    .Properties(ps => ps
                        .Text(s => s.Name(n => n.Value))
                        .Keyword(s => s.Name(n => n.XPath))
                    )
                )
            );
        }

        private async Task BuildIndex(IEnumerable<Node> nodes)
        {
            var watch = new Stopwatch();
            watch.Restart();
            await CreateIndexIfNotExist();
            watch.Stop();
            Console.WriteLine($"Creating the index took {watch.ElapsedMilliseconds}");

            watch.Restart();
            await _client.BulkAsync(b => b
                .Refresh(Refresh.True)
                .IndexMany(nodes)
            );
            watch.Stop();
            Console.WriteLine($"Loading the data into els took {watch.ElapsedMilliseconds}");
        }

        public async Task<Neighbors> FindNeighbors(
            IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            IMultiSearchRequest CreateSearch(MultiSearchDescriptor ms)
            {
                const float ratioLengthRange = 0.15f;
                foreach (var node in targetNodes)
                {
                    ms.Search<Node>(node.Id.ToString(), s => s
                        .Source(so => so.Includes(i => i.Field(n => n.Id)))
                        .Query(q => q
                                        .Match(m => m.Field(n => n.Value).Query(node.Value)) || q
                                        .Term(m => m.Field(n => n.XPath).Value(node.XPath))  || q
                                        .Range(m => m.Field(n => n.SizeValue)
                                            .GreaterThanOrEquals(node.SizeValue - ratioLengthRange * node.SizeValue)
                                            .LessThanOrEquals(node.SizeValue    + ratioLengthRange * node.SizeValue)) || q
                                        .Range(m => m.Field(n => n.SizeXPath)
                                            .GreaterThanOrEquals(node.SizeXPath - ratioLengthRange * node.SizeXPath)
                                            .LessThanOrEquals(node.SizeXPath    + ratioLengthRange * node.SizeXPath)
                                        ))
                    );
                }

                return ms;
            }

            await BuildIndex(sourceNodes);

            var stopwatch = new Stopwatch();
            stopwatch.Restart();
            var response = await _client.MultiSearchAsync(IndexName, CreateSearch);
            stopwatch.Stop();
            Console.WriteLine($"The search took {stopwatch.ElapsedMilliseconds} while els says it took: {response.Took}");
            var sourceNodesDictionary = sourceNodes.ToDictionary(n => n.Id);
            var neighbors             = new Neighbors();

            foreach (var targetNode in targetNodes)
            {
                var sourceNodesMatched = response.GetResponse<Node>(targetNode.Id.ToString())
                    ?.Hits
                    ?.Select(hit => new Scored<Node>(sourceNodesDictionary[hit.Source.Id], (float?) hit.Score));
                if (sourceNodesMatched != null)
                    neighbors.Add(targetNode, new HashSet<Scored<Node>>(sourceNodesMatched));
            }

            return neighbors;
        }
    }
}