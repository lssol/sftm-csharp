using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tree_matching_csharp
{
    public class TreeMatcherResponse
    {
        public IEnumerable<Edge> Edges { get; set; }
        public double Cost { get; set; }
        public long ComputationTime { get; set; }
    }
    public interface ITreeMatcher
    {
        public Task<TreeMatcherResponse?> MatchTrees(IEnumerable<Node> source, IEnumerable<Node> target);
    }
}