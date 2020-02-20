using System.Threading.Tasks;
using NUnit.Framework;

namespace tree_matching_csharp.Test
{
    public class TreeMatchingTest
    {
        [Test]
        public async Task TestTreeMatching()
        {
            var treeMatcher = new TreeMatcher(new TreeMatcher.Parameters
            {
               LimitNeighbors = 10,
               WeightsPropagation = new []{0.3f, 0.1f},
               MetropolisParameters = new Metropolis.Parameters
               {
                   Gamma = 0.7f,
                   Lambda = 0.7f,
                   NbIterations = 100,
                   NoMatchCost = 0.25f
               }
            });
        }
    }
}