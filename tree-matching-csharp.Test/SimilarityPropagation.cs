using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NUnit.Framework;
using Neighbors = System.Collections.Generic.Dictionary<tree_matching_csharp.Node, System.Collections.Generic.HashSet<tree_matching_csharp.Scored<tree_matching_csharp.Node>>>;

namespace tree_matching_csharp.Test
{
    public class SimilarityPropagationTest
    {
        [Test]
        public async Task CheckThatSimilarityPropagationIsUseful()
        {
            var indexer = new Indexer();
            var weights = new[] {0.3f, 0.05f};

            var original = File.ReadAllText("websites/linkedin.html");
            var mutant   = File.ReadAllText("websites/linkedin_mutant.html");

            var originalTree = await DOM.WebpageToTree(original);
            var mutantTree   = await DOM.WebpageToTree(mutant);

            var originalNodeToSignatureDic = originalTree.Nodes.ToDictionary(n => n, n => n.Signature);
            var mutantNodeToSignatureDic   = mutantTree.Nodes.ToDictionary(n => n, n => n.Signature);


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

            var neighbors = await indexer.FindNeighbors(originalTree.Nodes, mutantTree.Nodes);
            var mistakesNoPropagation   = ComputeAccuracy(neighbors);
            
            SimilarityPropagation.PropagateSimilarity(neighbors, weights);
            var mistakesWithPropagation = ComputeAccuracy(neighbors);

            Console.WriteLine($"Number of mistakes no propagation: {mistakesNoPropagation} / {mutantTree.Nodes.Count}");
            Console.WriteLine($"Number of mistakes with propagation: {mistakesWithPropagation} / {mutantTree.Nodes.Count}");
        }
    }
}