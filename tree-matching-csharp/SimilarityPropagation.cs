using System;
using System.Collections.Generic;

namespace tree_matching_csharp
{
    public class SimilarityPropagation
    {
        public class Parameters
        {
            public double   Sibling    { get; set; }
            public double   SiblingInv { get; set; }
            public double   Parent     { get; set; }
            public double   ParentInv  { get; set; }
            public double[] Envelop    { get; set; }
        }

        private static void IncreaseScore(Neighbors neighbors, Node sourceNode, Node targetNode, double incr)
        {
            var hits = neighbors.Get(targetNode) ?? new Dictionary<Node, double>();
            var hitScore = hits.ContainsKey(sourceNode) ? hits[sourceNode] : 0;
            var newScore = hitScore + incr;
            if (Math.Abs(newScore) < 0.001) return;
            hits[sourceNode] = newScore;
            if (!neighbors.Value.ContainsKey(targetNode))
                neighbors.Value[targetNode] = hits;
        }

        public static Neighbors PropagateSimilarity(Neighbors neighbors, Parameters parameters, double currentEnvelop)
        {
            var newSimilarity = new Neighbors();
            foreach (var (targetNode, hits) in neighbors.Value)
            foreach (var (sourceNode, score) in hits)
            {
                var pSource = sourceNode.Parent;
                var pTarget = targetNode.Parent;
                if (pSource == null || pTarget == null)
                {
                    IncreaseScore(newSimilarity, sourceNode, targetNode, score);
                    continue;
                }

                var parentScore = neighbors.Score(pSource, pTarget);
                IncreaseScore(newSimilarity, sourceNode, targetNode, score + currentEnvelop * parentScore * parameters.Parent);
                IncreaseScore(newSimilarity, pSource, pTarget, parentScore + currentEnvelop * score * parameters.ParentInv);
                
                
            }

            return newSimilarity;
        }
    }
}