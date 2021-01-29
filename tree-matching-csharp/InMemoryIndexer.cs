using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace tree_matching_csharp
{
    public class Index
    {
        private readonly int _maxNeighborsPerNode;
        private readonly int _maxTokenAppearance;
        private readonly IDictionary<string, IList<Node>> _index;
        private readonly HashSet<Node> _nodes;
        private double IdfPrecomputation { get; set; }
        private readonly HashSet<string> _removedTokens;

        public Index(int maxNeighborsPerNode, int maxTokenAppearance)
        {
            _maxNeighborsPerNode = maxNeighborsPerNode;
            _maxTokenAppearance = maxTokenAppearance;
            _index = new Dictionary<string, IList<Node>>();
            _nodes = new HashSet<Node>();
            _removedTokens = new HashSet<string>();
        }

        public void Add(string token, Node node)
        {
            _nodes.Add(node);
            if (_removedTokens.Contains(token))
                return;
            if (_index.ContainsKey(token))
                _index[token].Add(node);
            else
                _index.Add(token, new List<Node> {node});

            if (_index[token].Count <= _maxTokenAppearance) 
                return;
            
            _removedTokens.Add(token);
            _index.Remove(token);
        }

        public Dictionary<Node, double> QueryIndex(IEnumerable<string> query)
        {
            var hits = new Dictionary<Node, double>();
            foreach (var token in query)
            {
                if (!_index.ContainsKey(token))
                    continue;
                var nodesWithToken = _index[token];
                var idf = IdfPrecomputation - Math.Log(nodesWithToken.Count);
                foreach (var node in nodesWithToken)
                {
                    if (hits.ContainsKey(node))
                        hits[node] += idf;
                    else
                        hits.Add(node, idf);
                }
            }

            return hits;
        }

        public IDictionary<string, IList<Node>> GetTokenDictionary() => _index;

        public Index PrecomputeIdf() { 
            IdfPrecomputation = Math.Log(_nodes.Count);
            return this;
        }

        public Dictionary<Node, double> TruncateResults(Dictionary<Node, double> hits)
        {
            return hits
                .OrderByDescending(hit => hit.Value)
                .Take(_maxNeighborsPerNode)
                .ToDictionary(hit => hit.Key, hit => hit.Value);
        }
    }
    
    public class InMemoryIndexer
    {
        private readonly int _maxNbNeighbor;
        private readonly int _maxTokenAppearance;

        public InMemoryIndexer(int maxNbNeighbor, int maxTokenAppearance)
        {
            _maxNbNeighbor = maxNbNeighbor;
            _maxTokenAppearance = maxTokenAppearance;
        }

        public Neighbors FindNeighbors(Index index, IEnumerable<Node> targetNodes)
        {
            var neighbors = new Neighbors();
            foreach (var targetNode in targetNodes)
            {
                var queryIndex = index.QueryIndex(targetNode.Value);
                var results = index.TruncateResults(queryIndex);
                if (results.Count != 0)
                    neighbors.Value[targetNode] = results;
            }

            return neighbors;
        }
        
        public Index BuildIndex(IEnumerable<Node> sourceNodes)
        {
            var index = new Index(_maxNbNeighbor, _maxTokenAppearance);
            foreach (var sourceNode in sourceNodes)
                foreach (var value in sourceNode.Value)
                    index.Add(value, sourceNode);

            index.PrecomputeIdf();
            return index;
        }
    }
}