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
                LimitNeighbors = 100,
                MetropolisParameters = new Metropolis.Parameters
                {
                    Gamma        = 1f,
                    Lambda       = 0.7f,
                    NbIterations = 50,
                },
                NoMatchCost = 0.2,
                PropagationParameters = new SimilarityPropagation.Parameters()
                {
                    Envelop    = new[] {0.7, 0.0},
                    // Envelop    = new[] {0.0},
                    Parent     = 0.0,
                    Sibling    = 0.0,
                    SiblingInv = 0.0,
                    ParentInv  = 0.5
                },
                MaxTokenAppearance = n => (int) Math.Sqrt(n)
            });

            var source = File.ReadAllText("websites/linkedin.html");
            var target = File.ReadAllText("websites/linkedin_mutant.html");

            watch.Restart();
            var matching = await treeMatcher.MatchWebsites(source, target);
            watch.Stop();
            Console.WriteLine($"Overall Matching the websites took: {watch.ElapsedMilliseconds}");

            Console.WriteLine($"No match: {matching.NbNoMatch}");
            Console.WriteLine($"Nomatch unjusitified: {matching.NbNoMatchUnjustified}, mismatch: {matching.NbMismatch}");
            Console.WriteLine($"The success ratio is: {matching.GoodMatches / (double) matching.MaxGoodMatches}");
        }
    }
}