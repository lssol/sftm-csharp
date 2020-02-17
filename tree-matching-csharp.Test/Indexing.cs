using System.Collections.Generic;
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
            
            var exampleSource = sourceNodes[0];
            if (!neighbors[exampleSource].Contains(targetNodes[0]))
                Assert.Fail();
        }

        [Test]
        void FindNeighborsForFakeWebsite()
        {
//            DomTests.SimpleWebpage
        }
    }
}