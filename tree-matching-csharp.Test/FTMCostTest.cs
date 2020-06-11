using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using NUnit.Framework;
using tree_matching_csharp.Benchmark;

namespace tree_matching_csharp.Test
{
    public class FTMCostTest
    {
        [Test]
        public async Task TestFTMCost()
        {
            var sftmParam   = TreeMatchingTest._parameters;
            var treeMatcher = new SftmTreeMatcher(sftmParam);

            var source = File.ReadAllText("websites/linkedin.html");
            var target = File.ReadAllText("websites/linkedin.html");

            var websiteMatcher = new WebsiteMatcher(treeMatcher);
            var matching = await websiteMatcher.MatchWebsites(source, target);
            var cost = new FTMCost(matching.Matching).ComputeCost();
            
            Console.WriteLine(cost.ToJson());
            
            Assert.True(cost.Ancestry == 0);
            Assert.True(cost.Sibling == 0);
            Assert.True(cost.Relabel == 0);
            Assert.True(cost.NoMatch == 0);
        }
    }
}