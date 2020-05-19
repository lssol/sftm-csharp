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
        private readonly int _limitNeighbors;

        public Indexer(int limitNeighbors)
        {
            _limitNeighbors = limitNeighbors;
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

        private void CreateIndexIfNotExist()
        {
            var watch = new Stopwatch();
            watch.Restart();
            if (_client.Indices.Exists(IndexName).Exists)
                 _client.Indices.Delete(IndexName);
            watch.Stop();
            Console.WriteLine($"Deleting previous index took: {watch.ElapsedMilliseconds}");
            
            watch.Restart();
             _client.Indices.Create(IndexName, c => c
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0)
                    .FileSystemStorageImplementation(FileSystemStorageImplementation.MMap)
                    .Analysis(a => a.Analyzers(an => an.Whitespace("white")))
                )
                .Map<Node>(m => m
                    .Properties(ps => ps
                        .Text(s => s.Name(n => n.Value).Analyzer("white"))
                        .Keyword(s => s.Name(n => n.XPath))
                    )
                )
            );
            
            watch.Stop();
            Console.WriteLine($"Creating the empty index took: {watch.ElapsedMilliseconds}");
        }

        private void BuildIndex(IEnumerable<Node> nodes)
        {
            var watch = new Stopwatch();
            watch.Restart();
            CreateIndexIfNotExist();
            watch.Stop();
            Console.WriteLine($"Creating the index took {watch.ElapsedMilliseconds}");

            watch.Restart();
            _client.Bulk(b => b
                .IndexMany(nodes)
                .SourceEnabled(false)
                .Pretty(false)
                .ErrorTrace(false)
            );
            watch.Stop();
            _client.Indices.Refresh(IndexName);
            Console.WriteLine($"Loading the data into els took {watch.ElapsedMilliseconds}");
        }

        public Neighbors FindNeighbors(
            IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            IMultiSearchRequest CreateSearch(MultiSearchDescriptor ms)
            {
                const float ratioLengthRange = 0.15f;
                foreach (var node in targetNodes)
                {
                    ms.Search<Node>(node.Id.ToString(), s => s
                        .Pretty(false)
                        .Human(false)
                        .ErrorTrace(false)
                        .Source(so => so.Includes(i => i.Field(n => n.Id)))
                        .Size(_limitNeighbors)
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

            BuildIndex(sourceNodes);

            var stopwatch = new Stopwatch();
            stopwatch.Restart();
            var response = _client.MultiSearch(IndexName, CreateSearch);
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