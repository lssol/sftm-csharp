using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace tree_matching_csharp.indexers
{
    public class Index
    {
        private readonly IDictionary<string, IList<Node>> _index;
        private readonly HashSet<Node> _nodes;
        private double IdfPrecomputation { get; set; }

        public Index()
        {
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
        
        public HashSet<Scored<Node>> QueryIndex(IEnumerable<string> query)
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

            return new HashSet<Scored<Node>>(hits.Select(hit => new Scored<Node>(hit.Key, (float) hit.Value) ));
        }

        public Index PrecomputeIdf() { 
            IdfPrecomputation = Math.Log(_nodes.Count);
            return this;
        }
    }
    
    public class InMemoryIndexer : IIndexer
    {
        private Index  BuildIndex(IEnumerable<Node> sourceNodes)
        {
            var index = new Index();
            foreach (var sourceNode in sourceNodes)
                foreach (var value in sourceNode.Value)
                    index.Add(value, sourceNode);

            return index;
        }

        public IDictionary<Node, HashSet<Scored<Node>>> FindNeighbors(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            var index = BuildIndex(sourceNodes).PrecomputeIdf();
            
            var neighbors = new Dictionary<Node, HashSet<Scored<Node>>>();
            foreach (var targetNode in targetNodes)
                neighbors[targetNode] = index.QueryIndex(targetNode.Value);

            return neighbors;
        }
    }
}