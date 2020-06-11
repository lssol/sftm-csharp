using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NUnit.Framework;
using tree_matching_csharp.Benchmark;

namespace tree_matching_csharp.Test
{
    public class BenchmarkTest
    {
        [Test]
        public async Task TestRTED()
        {
            var rted = new RtedTreeMatcher(new RtedTreeMatcher.Parameters
            {
                DeletionCost  = 1,
                InsertionCost = 1,
                RelabelCost   = 1
            });

            var webpage = DomTests.SimpleWebpage;
            var nodes = await DOM.WebpageToTree(webpage);
            var res = await rted.MatchTrees(nodes, nodes);
            if (!res.Edges.Any())
                Assert.Fail();
            
            if (res.Edges.Any(m => m.Source != m.Target))
                Assert.Fail();
        }

        [Test]
        public void TestBracketParser()
        {
            var bracketTree = "{cesare abba strasse{1}{2}{3{{1}{3}}}{11}}";
            var parser = new PegParser.BracketParser();
            var tree = parser.Parse(bracketTree);
            Assert.True(tree != null);
            Assert.True(tree.Children.Count() == 4);
        }
        [Test]
        public void TestBolzano()
        {
            var trees = BolzanoImporter.GetBolzanoTrees();
            Assert.True(trees.Any());
        }
    }
}