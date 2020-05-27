using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace tree_matching_csharp
{
    public class Neighbors
    {
        public  Dictionary<Node, Dictionary<Node, double>> Value;
        private IList<Edge>           _edges;

        public Neighbors()
        {
            Value = new Dictionary<Node, Dictionary<Node, double>>();
        }

        public Dictionary<Node, double> Get(Node node) => Value.ContainsKey(node) ? Value[node] : null;

        public double Score(Node sourceNode, Node targetNode) => (Get(targetNode)?.ContainsKey(sourceNode) ?? false) ? Value[targetNode][sourceNode] : 0;

        public IEnumerable<Edge> GetEdges()
        {
            if (_edges == null)
                NeighborsToEdges();
            
            return _edges;
        }

        private void NeighborsToEdges()
        {
            _edges = new List<Edge>();
            
            foreach (var (targetNode, hits) in Value)
            foreach (var (sourceNode, score) in hits)
            {
                _edges.Add(new Edge
                {
                    Source = sourceNode,
                    Target = targetNode,
                    Cost = 1 / (1 + score)
                });
            }
        }
    }
}