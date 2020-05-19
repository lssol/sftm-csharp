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
            var sourceNodes = sourceList.Select(s => new Node { Value = s, XPath = s}).ToArray();
            var targetNodes = targetList.Select(s => new Node { Value = s, XPath = s}).ToArray();
            
            var indexer = new Indexer(10);
            var neighbors = indexer.FindNeighbors(sourceNodes, targetNodes);
            
            if (neighbors.Count == 0)
                Assert.Fail();
            
            var exampleTarget = targetNodes[0];
            var nodesAssociated = new HashSet<Node>(neighbors[exampleTarget].Select(n => n.Value));
            if (!nodesAssociated.Contains(sourceNodes[0]))
                Assert.Fail();
        }

        [Test]
        public async Task FindNeighborsForFakeWebsite()
        {
            var webpage = DomTests.SimpleWebpage;
            var nodes = await DOM.WebpageToTree(webpage);
            var indexer = new Indexer(10);
            var neighbors = indexer.FindNeighbors(nodes, nodes);
            if (neighbors.Count == 0)
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
           
            var indexer = new Indexer(10);

            stopwatch.Restart();
            var neighbors = indexer.FindNeighbors(nodes, nodes);
            stopwatch.Stop();
            Console.WriteLine($"find neighbors took: {stopwatch.ElapsedMilliseconds}");
            
            if (neighbors.Count == 0)
                Assert.Fail();
            var mistakes = 0;
            foreach (var (target, matchedSources) in neighbors)
            {
                var bestMatch = matchedSources.MaxBy(n => n.Score).First().Value;
                if (target != bestMatch)
                    mistakes++;
            }
            Console.WriteLine($"Mistakes: {mistakes} / {nodes.Count()}");
            
            if (mistakes > 0.9 * nodes.Count())
                Assert.Fail();
        }
    }
}