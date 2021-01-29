using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;
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
            var sourceNodes = sourceList.Select(s => new Node { Value = s.Split(" ").ToList()}).ToArray();
            var targetNodes = targetList.Select(s => new Node { Value = s.Split(" ").ToList()}).ToArray();
            
            var indexer = new InMemoryIndexer(10, 10);
            var index = indexer.BuildIndex(sourceNodes);
            var neighbors = indexer.FindNeighbors(index, targetNodes);
            
            if (neighbors.Value.Count == 0)
                Assert.Fail();
            
            var exampleTarget = targetNodes[0];
            var nodesAssociated = new HashSet<Node>(neighbors.Value[exampleTarget].Select(n => n.Key));
            
            Assert.True(nodesAssociated.Contains(sourceNodes[0]));
            Assert.True(nodesAssociated.Contains(sourceNodes[1]));
            Assert.True(nodesAssociated.Contains(sourceNodes[2]));
        }

        [Test]
        public async Task FindNeighborsForFakeWebsite()
        {
            var webpage = DomTests.SimpleWebpage;
            var nodes = await DOM.WebpageToTree(webpage);
            var indexer = new InMemoryIndexer(100, 100);;
            var index = indexer.BuildIndex(nodes);
            var neighbors = indexer.FindNeighbors(index, nodes);
            if (neighbors.Value.Count == 0)
                Assert.Fail();
        }
        [Test]
        public async Task FindNeighborsForRealWebsite()
        {
            var stopwatch = new Stopwatch();
            
            var webpage = File.ReadAllText("websites/google.html");
            
            stopwatch.Restart();
            var nodes = await DOM.WebpageToTree(webpage);
            stopwatch.Stop();
            Console.WriteLine($"Webpage to tree took: {stopwatch.ElapsedMilliseconds}");
           
            var indexer = new InMemoryIndexer(100, 100);
            var index = indexer.BuildIndex(nodes);

            stopwatch.Restart();
            var neighbors = indexer.FindNeighbors(index, nodes);
            stopwatch.Stop();
            Console.WriteLine($"find neighbors took: {stopwatch.ElapsedMilliseconds}");
            
            if (neighbors.Value.Count == 0)
                Assert.Fail();
            var mistakes = 0;
            foreach (var (target, matchedSources) in neighbors.Value)
            {
                var bestMatch = matchedSources.MaxBy(n => n.Value).First().Key;
                if (target != bestMatch)
                    mistakes++;
            }
            Console.WriteLine($"Mistakes: {mistakes} / {nodes.Count()}");
            
            if (mistakes > 0.9 * nodes.Count())
                Assert.Fail();
        }
    }
}