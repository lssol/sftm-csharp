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
                LimitNeighbors = 50,
                MetropolisParameters = new Metropolis.Parameters
                {
                    Gamma        = 0.7f,
                    Lambda       = 0.7f,
                    NbIterations = 50,
                },
                NoMatchCost = 7,
                PropagationParameters = new SimilarityPropagation.Parameters()
                {
                    Envelop    = new[] {0.7, 0.1},
                    Parent     = 0.5,
                    Sibling    = 0.3,
                    SiblingInv = 0.1,
                    ParentInv  = 0.2
                },
                MaxTokenAppearance = n => (int) Math.Sqrt(n)
            });

            var source = File.ReadAllText("websites/linkedin.html");
            var target = File.ReadAllText("websites/linkedin_mutant.html");

            watch.Restart();
            var matching = await treeMatcher.MatchWebsites(source, target);
            watch.Stop();
            Console.WriteLine($"Overall Matching the websites took: {watch.ElapsedMilliseconds}");

            var total   = matching.SignatureMatching.Count();
            var success = total - matching.NbMismatch - matching.NbNoMatch;

            Console.WriteLine($"Nomatch: {matching.NbNoMatch}, mismatch: {matching.NbMismatch}");
            Console.WriteLine($"The success ratio is: {(float) success / total}");
        }
    }
}