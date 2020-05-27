using System.Collections.Generic;
using System.Linq;

namespace tree_matching_csharp
{
    public class Neighbors
    {
        public  Dictionary<Node, Dictionary<Node, double>> Value;
        // private Dictionary<(Node, Node), double>           _edges;
        private IList<Edge>           _edges;

        public Neighbors()
        {
            Value = new Dictionary<Node, Dictionary<Node, double>>();
        }

        public void InvalidateCache()
        {
            _edges = null;
            // _sortedEdges = null;
        }

        // public Dictionary<(Node, Node), double> GetEdges()
        // {
        //     if (_edges == null)
        //         NeighborsToEdges();
        //     
        //     return _edges;
        // }
        
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