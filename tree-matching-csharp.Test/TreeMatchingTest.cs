using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace tree_matching_csharp.Test
{
    public class TreeMatchingTest
    {
        [Test]
        public async Task TestTreeMatching()
        {
            var watch = new Stopwatch();
            var treeMatcher = new TreeMatcher(new TreeMatcher.Parameters
            {
                LimitNeighbors     = 10,
                WeightsPropagation = new[] {0.3f, 0.1f},
                MetropolisParameters = new Metropolis.Parameters
                {
                    Gamma        = 0.7f,
                    Lambda       = 0.7f,
                    NbIterations = 100,
                }
            });

            var source = File.ReadAllText("websites/linkedin.html");
            var target = File.ReadAllText("websites/linkedin_mutant.html");
            
            watch.Restart();
            var matching = await treeMatcher.MatchWebsites(source, target);
            watch.Stop();
            Console.WriteLine($"Overall Matching the websites took: {watch.ElapsedMilliseconds}");

            var success = matching.Count(m => m.Item1 == m.Item2);
            var total   = matching.Count();

            Console.WriteLine($"The success ratio is: {(float) success / total}");
        }
    }
}