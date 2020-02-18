using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;

namespace tree_matching_csharp
{
    public class Indexer
    {
        private const    string        IndexName = "wehave-node";
        private readonly ElasticClient _client;
        private          Task          _deleting;

        private Node[] _nodes;

        public Indexer()
        {
            var settings = new ConnectionSettings()
                .DefaultIndex(IndexName)
//                .EnableDebugMode()
                .DefaultMappingFor<Node>(m => m
                    .IdProperty(n => n.Id)
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
            await CreateIndexIfNotExist();
            await _client.BulkAsync(b => b
                .Refresh(Refresh.True)
                .IndexMany(nodes)
            );
        }

        public async Task<Neighbors> FindNeighbors(
            IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            IMultiSearchRequest CreateSearch(MultiSearchDescriptor ms)
            {
                foreach (var node in targetNodes)
                {
                    ms.Search<Node>(node.Id.ToString(), s => s
                        .Source(so => so.Includes(i => i.Field(n => n.Id)))
                        .Query(q => q
                            .Match(m => m.Field(n => n.Value).Query(node.Value))
                        )
                        .Query(q => q
                            .Term(m => m.Field(n => n.XPath).Value(node.XPath))
                        )
                    );
                }

                return ms;
            }

            await BuildIndex(sourceNodes);
            var response              = await _client.MultiSearchAsync(IndexName, CreateSearch);
            var sourceNodesDictionary = sourceNodes.ToDictionary(n => n.Id);
            var neighbors             = new Neighbors();
            
            foreach (var targetNode in targetNodes)
            {
                var sourceNodesMatched = response.GetResponse<Node>(targetNode.Id.ToString())
                    ?.Hits
                    ?.Select(hit => new Scored<Node>(sourceNodesDictionary[hit.Source.Id], hit.Score));
                if (sourceNodesMatched != null)
                    neighbors.Add(targetNode, new HashSet<Scored<Node>>(sourceNodesMatched));
            }

            return neighbors;
        }
    }
}