using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

namespace tree_matching_csharp
{
    public class SftmTreeMatcher: ITreeMatcher
    {
        public class Parameters
        {
            public SimilarityPropagation.Parameters PropagationParameters { get; set; }
            public Metropolis.Parameters            MetropolisParameters  { get; set; }
            public double                           NoMatchCost           { get; set; }
            public int                              LimitNeighbors        { get; set; }
            public Func<int, int>                   MaxTokenAppearance    { get; set; }
        }


        private readonly Parameters _param;

        public SftmTreeMatcher(Parameters param)
        {
            _param = param;
        }


        private IEnumerable<Edge> GetNoMatchEdges(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            var edgesFromSource = sourceNodes.Select(n => new Edge {Cost = _param.NoMatchCost, Source = n, Target    = null});
            var edgesFromTarget = targetNodes.Select(n => new Edge {Cost = _param.NoMatchCost, Source = null, Target = n});

            return edgesFromSource.Concat(edgesFromTarget);
        }

        public Task<TreeMatcherResponse> MatchTrees(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            var watch = new Stopwatch();
            
            watch.Start();
            var indexer = new InMemoryIndexer(_param.LimitNeighbors, _param.MaxTokenAppearance(sourceNodes.Count()));

            var neighbors = indexer.FindNeighbors(sourceNodes, targetNodes);
            _param.PropagationParameters.Envelop.ForEach(envelop => { neighbors = SimilarityPropagation.PropagateSimilarity(neighbors, _param.PropagationParameters, envelop); });

            var noMatchEdges = GetNoMatchEdges(sourceNodes, targetNodes);
            var edges        = neighbors.GetEdges().Concat(noMatchEdges);
            var metropolis   = new Metropolis(_param.MetropolisParameters, edges, sourceNodes.Count() + targetNodes.Count(), _param.LimitNeighbors);

            var matchingEdges = metropolis.Run();
            watch.Stop();

            return Task.FromResult(new TreeMatcherResponse
            {
                Edges           = matchingEdges,
                ComputationTime = watch.ElapsedMilliseconds
            });
        }

    }
}