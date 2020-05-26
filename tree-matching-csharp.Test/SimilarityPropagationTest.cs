using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NUnit.Framework;
using tree_matching_csharp.indexers;
using Neighbors = System.Collections.Generic.IDictionary<tree_matching_csharp.Node, System.Collections.Generic.HashSet<tree_matching_csharp.Scored<tree_matching_csharp.Node>>>;

namespace tree_matching_csharp.Test
{
    public class SimilarityPropagationTest
    {
        [Test]
        public async Task CheckThatSimilarityPropagationIsUseful()
        {
            var stopWatch = new Stopwatch();
            var indexer = new InMemoryIndexer();
            var weights = new[] {0.3f, 0.05f};

            var original = File.ReadAllText("websites/linkedin.html");
            var mutant   = File.ReadAllText("websites/linkedin_mutant.html");

            stopWatch.Restart();
            var originalNodes = await DOM.WebpageToTree(original);
            var mutantNodes   = await DOM.WebpageToTree(mutant);
            stopWatch.Stop();
            Console.WriteLine($"Building Trees took: {stopWatch.ElapsedMilliseconds}");

            var originalNodeToSignatureDic = originalNodes.ToDictionary(n => n, n => n.Signature);
            var mutantNodeToSignatureDic   = mutantNodes.ToDictionary(n => n, n => n.Signature);

            int ComputeAccuracy(Neighbors neighbors)
            {
                var mistakes = 0;
                foreach (var (targetNode, matchedNodes) in neighbors)
                {
                    var bestMatch = matchedNodes.MaxBy(n => n.Score).FirstOrDefault()?.Value;
                    if (bestMatch == null || originalNodeToSignatureDic[bestMatch] != mutantNodeToSignatureDic[targetNode])
                        mistakes++;
                }

                return mistakes;
            }

            stopWatch.Restart();
            var neighbors = indexer.FindNeighbors(originalNodes, mutantNodes);
            stopWatch.Stop();
            Console.WriteLine($"Finding Neighbors took: {stopWatch.ElapsedMilliseconds}");
            
            var mistakesNoPropagation   = ComputeAccuracy(neighbors);
            
            stopWatch.Restart();
            SimilarityPropagation.PropagateSimilarity(neighbors, weights);
            stopWatch.Stop();
            var mistakesWithPropagation = ComputeAccuracy(neighbors);
            Console.WriteLine($"Propagating the similarity took: {stopWatch.ElapsedMilliseconds}");
            
            Console.WriteLine($"Number of mistakes no propagation: {mistakesNoPropagation} / {mutantNodes.Count()}");
            Console.WriteLine($"Number of mistakes with propagation: {mistakesWithPropagation} / {mutantNodes.Count()}");
        }
    }
}