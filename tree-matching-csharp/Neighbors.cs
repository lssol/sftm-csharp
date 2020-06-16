using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace tree_matching_csharp
{
    public class Neighbors
    {
        public  Dictionary<Node, Dictionary<Node, double>> Value;

        public Neighbors()
        {
            Value = new Dictionary<Node, Dictionary<Node, double>>();
        }

        public Dictionary<Node, double> Get(Node node) => Value.ContainsKey(node) ? Value[node] : null;

        public double Score(Node sourceNode, Node targetNode) => (Get(targetNode)?.ContainsKey(sourceNode) ?? false) ? Value[targetNode][sourceNode] : 0;

        public IEnumerable<Edge> ToEdges()
        {
            var edges = new List<Edge>();
            
            foreach (var (targetNode, hits) in Value)
            foreach (var (sourceNode, score) in hits)
            {
                edges.Add(new Edge
                {
                    Source = sourceNode,
                    Target = targetNode,
                    Score = score
                });
            }

            return edges;
        }
    }
}