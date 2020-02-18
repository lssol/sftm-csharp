using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Common;
using MoreLinq.Extensions;
using Neighbors = System.Collections.Generic.Dictionary<tree_matching_csharp.Node, System.Collections.Generic.HashSet<tree_matching_csharp.Scored<tree_matching_csharp.Node>>>;

namespace tree_matching_csharp
{
    public static class SimilarityPropagation
    {
        public static IEnumerable<Edge> NeighborsToEdges(Neighbors neighbors)
        {
            return neighbors.SelectMany(pair => pair
                .Value
                .Select(source => new Edge
                {
                    Cost   = source.Score,
                    Source = source.Value,
                    Target = pair.Key
                }));
        }

        public static Neighbors PropagateSimilarity(Neighbors neighbors, float[] propagationWeights)
        {
            var edges    = NeighborsToEdges(neighbors);
            var edgesDic = edges.ToDictionary(e => (e.Source, e.Target), e => e.Cost);

            float ComputePropagatedCost(Node source, Node target, float cost)
            {
                var result = cost;
                propagationWeights.ForEach((w, i) =>
                {
                    var p1 = Utils.GetNthParent(source, i);
                    var p2 = Utils.GetNthParent(target, i);
                    if (!edgesDic.ContainsKey((p1, p2)))
                        return;
                    var parentCost = edgesDic[(p1, p2)] ?? 0;
                    result += w * parentCost;
                });
                return result;
            }

            return neighbors.ToDictionary(pair => pair.Key, pair =>
            {
                var (target, sources) = pair;
                return new HashSet<Scored<Node>>(sources.Select(source =>
                {
                    source.Score = ComputePropagatedCost(source.Value, target, source.Score ?? 0);
                    return source;
                }));
            });
        }
    }
}