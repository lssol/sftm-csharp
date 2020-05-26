using System.Collections.Generic;
using Neighbors = System.Collections.Generic.IDictionary<tree_matching_csharp.Node, System.Collections.Generic.HashSet<tree_matching_csharp.Scored<tree_matching_csharp.Node>>>;

namespace tree_matching_csharp.indexers
{
    public interface IIndexer
    {
        public Neighbors FindNeighbors(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes);
    }
}