using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using tree_matching_csharp.Benchmark;

namespace tree_matching_csharp.Test
{
    public class TreeMatchingTest
    {
        public static SftmTreeMatcher.Parameters _parameters = new SftmTreeMatcher.Parameters
        {
            LimitNeighbors = 100,
            MetropolisParameters = new Metropolis.Parameters
            {
                Gamma        = 0.9f,
                Lambda       = 0.7f,
                NbIterations = 10,
            },
            NoMatchCost                    = 1.2,
            MaxPenalizationChildren        = 0.4,
            MaxPenalizationParentsChildren = 0.2,
            PropagationParameters = new SimilarityPropagation.Parameters()
            {
                Envelop = new[] {0.8, 0.1, 0.01},
                // Envelop    = new[] {0.0},
                Parent     = 0.25,
                Sibling    = 0.0,
                SiblingInv = 0.0,
                ParentInv  = 0.7,
                Children   = 0.1
            },
            MaxTokenAppearance = n => (int) Math.Sqrt(n)
        };

        [Test]
        public async Task TestTreeMatching()
        {
            var watch       = new Stopwatch();
            var treeMatcher = new SftmTreeMatcher(_parameters);

            var source = File.ReadAllText("websites/linkedin.html");
            var target = File.ReadAllText("websites/linkedin_mutant.html");

            var websiteMatcher = new WebsiteMatcher(treeMatcher);
            watch.Restart();
            var matching = await websiteMatcher.MatchWebsites(source, target);
            watch.Stop();
            Console.WriteLine($"Overall Matching the websites took: {watch.ElapsedMilliseconds}");
            Console.WriteLine($"No match: {matching.NbNoMatch}");
            Console.WriteLine($"Nomatch unjusitified: {matching.NbNoMatchUnjustified}, mismatch: {matching.NbMismatch}");
            Console.WriteLine($"The success ratio is: {matching.GoodMatches / (double) matching.MaxGoodMatches}");
        }

        [Test]
        public async Task TestXyDiff()
        {
            var watch       = new Stopwatch();
            var xyDiff = new XyDiffMatcher();

            var source = File.ReadAllText("websites/linkedin.html");
            var target = File.ReadAllText("websites/linkedin_mutant.html");

            watch.Restart();
            var matching = await xyDiff.MatchWebsites(source, target);
            watch.Stop();
            Console.WriteLine($"Overall Matching the websites took: {watch.ElapsedMilliseconds}");

            Console.WriteLine($"No match: {matching.NbNoMatch}");
            Console.WriteLine($"Nomatch unjusitified: {matching.NbNoMatchUnjustified}, mismatch: {matching.NbMismatch}");
            Console.WriteLine($"The success ratio is: {matching.GoodMatches / (double) matching.MaxGoodMatches}");
        }
    }
}