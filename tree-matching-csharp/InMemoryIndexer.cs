using System;
using System.Collections.Generic;
using System.Linq;

namespace tree_matching_csharp
{
    public class Index
    {
        private readonly int _maxNeighborsPerNode;
        private readonly IDictionary<string, IList<Node>> _index;
        private readonly HashSet<Node> _nodes;
        private double IdfPrecomputation { get; set; }

        public Index(int maxNeighborsPerNode)
        {
            _maxNeighborsPerNode = maxNeighborsPerNode;
            _index = new Dictionary<string, IList<Node>>();
            _nodes = new HashSet<Node>();
        }

        public void Add(string token, Node node)
        {
            _nodes.Add(node);
            if (_index.ContainsKey(token))
                _index[token].Add(node);
            else
                _index.Add(token, new List<Node> {node});
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

        public InMemoryIndexer(int maxNbNeighbor)
        {
            _maxNbNeighbor = maxNbNeighbor;
        }

        public Neighbors FindNeighbors(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            var index = BuildIndex(sourceNodes).PrecomputeIdf();
            
            var neighbors = new Neighbors();
            foreach (var targetNode in targetNodes)
                neighbors.Value[targetNode] = index.TruncateResults(index.QueryIndex(targetNode.Value));

            return neighbors;
        }
        
        private Index BuildIndex(IEnumerable<Node> sourceNodes)
        {
            var index = new Index(_maxNbNeighbor);
            foreach (var sourceNode in sourceNodes)
                foreach (var value in sourceNode.Value)
                    index.Add(value, sourceNode);

            return index;
        }
    }
}