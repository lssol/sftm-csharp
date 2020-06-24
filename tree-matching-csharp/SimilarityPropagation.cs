using System;
using System.Collections.Generic;
using MoreLinq;

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
            public double   Children  { get; set; }
            public double[] Envelop    { get; set; }
        }

        private static void IncreaseScore(Neighbors neighbors, Node sourceNode, Node targetNode, double incr)
        {
            if (Math.Abs(incr) < 0.001)
                return;
            var hits     = neighbors.Get(targetNode) ?? new Dictionary<Node, double>();
            var hitScore = hits.ContainsKey(sourceNode) ? hits[sourceNode] : 0;
            var newScore = hitScore + incr;
            if (Math.Abs(newScore) < 0.001)
                return;
            hits[sourceNode] = newScore;
            if (!neighbors.Value.ContainsKey(targetNode))
                neighbors.Value[targetNode] = hits;
        }

        public static Neighbors PropagateSimilarity(Neighbors neighbors, Parameters parameters, double currentEnvelop)
        {
            var newSimilarity = new Neighbors();
            void PropagateParenthood(Node sourceNode, Node targetNode)
            {
                var score = neighbors.Score(sourceNode, targetNode);
                targetNode
                    .Children
                    .ForEach(targetChild => targetChild.Children
                        .ForEach(sourceChild => IncreaseScore(newSimilarity, sourceChild, targetChild, score * parameters.Children * currentEnvelop)));
            }
            
            foreach (var (targetNode, hits) in neighbors.Value)
            foreach (var (sourceNode, score) in hits)
            {
                IncreaseScore(newSimilarity, sourceNode, targetNode, score);
                // PropagateParenthood(sourceNode, targetNode);
                
                var pSource = sourceNode.Parent;
                var pTarget = targetNode.Parent;

                if (pSource == null || pTarget == null) continue;

                var parentScore = neighbors.Score(pSource, pTarget);
                IncreaseScore(newSimilarity, sourceNode, targetNode, currentEnvelop * parentScore * parameters.Parent);
                IncreaseScore(newSimilarity, pSource,    pTarget,    currentEnvelop * score       * parameters.ParentInv);

                if (sourceNode.LeftSibling == null || targetNode.LeftSibling == null) continue;

                var siblingScore = neighbors.Score(sourceNode.LeftSibling, targetNode.LeftSibling);
                IncreaseScore(newSimilarity, sourceNode,             targetNode,             currentEnvelop * siblingScore * parameters.Sibling);
                IncreaseScore(newSimilarity, sourceNode.LeftSibling, targetNode.LeftSibling, currentEnvelop * score        * parameters.SiblingInv);
            }

            return newSimilarity;
        }
    }
}