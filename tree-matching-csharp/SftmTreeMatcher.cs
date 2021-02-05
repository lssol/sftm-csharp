using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Common;
using MoreLinq;

namespace tree_matching_csharp
{
    public class SftmTreeMatcher : ITreeMatcher
    {
        public class Parameters
        {
            public SimilarityPropagation.Parameters PropagationParameters          { get; set; }
            public Metropolis.Parameters            MetropolisParameters           { get; set; }
            public double                           NoMatchCost                    { get; set; }
            public int                              LimitNeighbors                 { get; set; }
            public Func<int, int>                   MaxTokenAppearance             { get; set; }
            public double                           MaxPenalizationChildren        { get; set; }
            public double                           MaxPenalizationParentsChildren { get; set; }
        }


        private readonly Parameters _param;

        public SftmTreeMatcher(Parameters param)
        {
            _param = param;
        }

        public void AddParentToken(IEnumerable<Node> nodes, IDictionary<string, IList<Node>> index)
        {
            var rarestToken = nodes
                .ToDictionary(n => n, n => n.Value
                    .Where(v => !v.StartsWith("/BODY"))
                    .MinBy(token => index.GetOrDefault(token, null)?.Count ?? nodes.Count()).First());

            nodes.ForEach(n =>
            {
                if (n.Parent != null && rarestToken.ContainsKey(n.Parent))
                    n.Value.Add($"#C_{rarestToken[n.Parent]}");
            });
        }

        public Task<TreeMatcherResponse> MatchTrees(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            var watch = new Stopwatch();
            watch.Start();
            sourceNodes.ComputeChildren();
            targetNodes.ComputeChildren();

            var indexer     = new InMemoryIndexer(_param.LimitNeighbors, _param.MaxTokenAppearance(sourceNodes.Count()));
            var index       = indexer.BuildIndex(sourceNodes);
            var indexTarget = indexer.BuildIndex(targetNodes);
            
            AddParentToken(sourceNodes, index.GetTokenDictionary());
            AddParentToken(targetNodes, indexTarget.GetTokenDictionary());
            
            index = indexer.BuildIndex(sourceNodes);

            var neighbors = indexer.FindNeighbors(index, targetNodes);
            ComputeChildrenPenalization(neighbors, sourceNodes.Concat(targetNodes));
            _param.PropagationParameters.Envelop.ForEach(envelop => { neighbors = SimilarityPropagation.PropagateSimilarity(neighbors, _param.PropagationParameters, envelop); });

            var noMatchEdges = GetNoMatchEdges(sourceNodes, targetNodes);
            var edges        = neighbors.ToEdges().Concat(noMatchEdges);
            var w2 = new Stopwatch();
            w2.Start();
            var metropolis   = new Metropolis(_param.MetropolisParameters, edges, sourceNodes.Count() + targetNodes.Count(), _param.LimitNeighbors);

            var matchingEdges = metropolis.Run();
            w2.Stop();
            watch.Stop();

            var cost = new FtmCost(matchingEdges).ComputeCost();

            return Task.FromResult(new TreeMatcherResponse
            {
                Edges           = matchingEdges,
                ComputationTime = watch.ElapsedMilliseconds,
                Cost = cost.Total
            });
        }

        private void ComputeChildrenPenalization(Neighbors neighbors, IEnumerable<Node> nodes)
        {
            var edges                = neighbors.ToEdges();
            var averageChildrenCount = nodes.Average(n => n.Children.Count);

            double ComputeRatioChildren(int cT, int cS, double maxPenalization)
            {
                var ratioChildren = Math.Abs(cS - cT) / averageChildrenCount;
                var cappedRatio   = Math.Max(1, ratioChildren);
                return cappedRatio * maxPenalization;
            }

            foreach (var edge in edges)
            {
                var cT = edge.Target.Children.Count;
                var cS = edge.Target.Children.Count;

                var cpT = edge.Target.Parent?.Children?.Count ?? 0;
                var cpS = edge.Target.Parent?.Children?.Count ?? 0;

                neighbors.Value[edge.Target][edge.Source] = neighbors.Value[edge.Target][edge.Source] * (1                                                              -
                                                                                                         ComputeRatioChildren(cT,  cS,  _param.MaxPenalizationChildren) -
                                                                                                         ComputeRatioChildren(cpT, cpS, _param.MaxPenalizationParentsChildren));
            }
        }

        private IEnumerable<Edge> GetNoMatchEdges(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            var edgesFromSource = sourceNodes.Select(n => new Edge {Score = 1 / _param.NoMatchCost, Source = n, Target    = null});
            var edgesFromTarget = targetNodes.Select(n => new Edge {Score = 1 / _param.NoMatchCost, Source = null, Target = n});

            return edgesFromSource.Concat(edgesFromTarget);
        }
    }
}