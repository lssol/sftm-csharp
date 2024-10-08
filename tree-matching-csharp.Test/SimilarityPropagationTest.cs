﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NUnit.Framework;

namespace tree_matching_csharp.Test
{
    public class SimilarityPropagationTest
    {
        [Test]
        public async Task CheckThatSimilarityPropagationIsUseful()
        {
            var stopWatch = new Stopwatch();
            var indexer = new InMemoryIndexer(200, 200);
            var parameters = new SimilarityPropagation.Parameters()
            {
                Envelop    = new [] {0.7, 0.05, 0.01},
                Parent     = 0.6,
                Sibling    = 0.2,
                SiblingInv = 0.1,
                ParentInv  = 0.4
            };

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
                foreach (var (targetNode, matchedNodes) in neighbors.Value)
                {
                    var bestMatch = matchedNodes.MaxBy(n => n.Value).FirstOrDefault().Key;
                    if (bestMatch == null || originalNodeToSignatureDic[bestMatch] != mutantNodeToSignatureDic[targetNode])
                        mistakes++;
                }

                return mistakes;
            }

            var index = indexer.BuildIndex(originalNodes);
            stopWatch.Restart();
            var neighbors = indexer.FindNeighbors(index, mutantNodes);
            stopWatch.Stop();
            Console.WriteLine($"Finding Neighbors took: {stopWatch.ElapsedMilliseconds}");
            
            var mistakesNoPropagation   = ComputeAccuracy(neighbors);
            
            stopWatch.Restart();
            parameters.Envelop.ForEach(envelop => neighbors = SimilarityPropagation.PropagateSimilarity(neighbors, parameters, envelop));
            stopWatch.Stop();
            var mistakesWithPropagation = ComputeAccuracy(neighbors);
            Console.WriteLine($"Propagating the similarity took: {stopWatch.ElapsedMilliseconds}");
            
            Console.WriteLine($"Number of mistakes no propagation: {mistakesNoPropagation} / {mutantNodes.Count()}");
            Console.WriteLine($"Number of mistakes with propagation: {mistakesWithPropagation} / {mutantNodes.Count()}");
            Assert.True(mistakesNoPropagation > mistakesWithPropagation);
        }
    }
}