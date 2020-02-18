using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace tree_matching_csharp.Test
{
    public class Indexing
    {
        [Test]
        public async Task FindNeighbors()
        {
            var sourceList = new List<string>()
            {
                "a node",
                "another node",
                "node",
                "a fox",
                "I love fox",
                "fox"
            };
            var targetList = new List<string>
            {
                "node",
                "fox"
            };
            var sourceNodes = sourceList.Select(s => new Node { Value = s }).ToArray();
            var targetNodes = targetList.Select(s => new Node { Value = s }).ToArray();
            
            var indexer = new Indexer();
            var neighbors = await indexer.FindNeighbors(sourceNodes, targetNodes);
            
            if (neighbors.Count == 0)
                Assert.Fail();
            
            var exampleTarget = targetNodes[0];
            var nodesAssociated = new HashSet<Node>(neighbors[exampleTarget].Select(n => n.Node));
            if (!nodesAssociated.Contains(sourceNodes[0]))
                Assert.Fail();
        }

        [Test]
        public async Task FindNeighborsForFakeWebsite()
        {
            var webpage = DomTests.SimpleWebpage;
            var tree = await DOM.WebpageToTree(webpage);
            var indexer = new Indexer();
            var neighbors = await indexer.FindNeighbors(tree.Nodes, tree.Nodes);
            if (neighbors.Count == 0)
                Assert.Fail();
        }
        [Test]
        public async Task FindNeighborsForRealWebsite()
        {
            var webpage = File.ReadAllText("youtube.html");
            var tree = await DOM.WebpageToTree(webpage);
            var indexer = new Indexer();
            var neighbors = await indexer.FindNeighbors(tree.Nodes, tree.Nodes);
            if (neighbors.Count == 0)
                Assert.Fail();
            
        }
    }
}