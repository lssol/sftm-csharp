using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

namespace tree_matching_csharp
{
    public class TreeMatcher
    {
        public class Parameters
        {
            public SimilarityPropagation.Parameters PropagationParameters { get; set; }
            public Metropolis.Parameters            MetropolisParameters  { get; set; }
            public double                           NoMatchCost           { get; set; }
            public int                              LimitNeighbors        { get; set; }
            public Func<int, int>                   MaxTokenAppearance    { get; set; }
        }

        public class Result
        {
            public IEnumerable<(string, string)> SignatureMatching    { get; set; }
            public int                           NbMismatch           { get; set; }
            public int                           NbNoMatch            { get; set; }
            public int                           NbNoMatchUnjustified { get; set; }
            public long                          ComputationTime      { get; set; }
            public int                           Total                { get; set; }
            public int MaxGoodMatches { get; set; }
            public int GoodMatches { get; set; }
        }

        private readonly Parameters _param;

        public TreeMatcher(Parameters param)
        {
            _param = param;
        }

        private static bool IsSignaturePresent(IEnumerable<Node> nodes)
        {
            var nullSignatures = nodes.Where(n => n.Signature == null);
            return nullSignatures.Count() <= 0.2 * nodes.Count();
        }

        public IEnumerable<Edge> GetNoMatchEdges(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            var edgesFromSource = sourceNodes.Select(n => new Edge {Cost = _param.NoMatchCost, Source = n, Target    = null});
            var edgesFromTarget = targetNodes.Select(n => new Edge {Cost = _param.NoMatchCost, Source = null, Target = n});

            return edgesFromSource.Concat(edgesFromTarget);
        }

        public IList<Edge> MatchTrees(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            var indexer = new InMemoryIndexer(_param.LimitNeighbors, _param.MaxTokenAppearance(sourceNodes.Count()));

            var neighbors = indexer.FindNeighbors(sourceNodes, targetNodes);
            _param.PropagationParameters.Envelop.ForEach(envelop => { neighbors = SimilarityPropagation.PropagateSimilarity(neighbors, _param.PropagationParameters, envelop); });

            var noMatchEdges = GetNoMatchEdges(sourceNodes, targetNodes);
            var edges        = neighbors.GetEdges().Concat(noMatchEdges);
            var metropolis   = new Metropolis(_param.MetropolisParameters, edges, sourceNodes.Count() + targetNodes.Count());

            return metropolis.Run();
        }

        public async Task<Result> MatchWebsites(string source, string target)
        {
            var watch = new Stopwatch();

            watch.Restart();
            var sourceNodes = await DOM.WebpageToTree(source);
            var targetNodes = await DOM.WebpageToTree(target);
            watch.Stop();
            // Console.WriteLine($"Creating trees took: {watch.ElapsedMilliseconds}");
            // Console.WriteLine($"The trees have {sourceNodes.Count()} and {targetNodes.Count()} nodes");

            var minCount = Math.Min(sourceNodes.Count(), targetNodes.Count());

            watch.Restart();
            var matching = MatchTrees(sourceNodes, targetNodes);
            watch.Stop();
            var computationTime = watch.ElapsedMilliseconds;

            if (!IsSignaturePresent(sourceNodes) || !IsSignaturePresent(targetNodes))
                throw new Exception("The web documents are expected to contain signature attributes");

            var commonSignatures = new HashSet<string>(sourceNodes.Select(n => n.Signature));
            var targetSignatures = new HashSet<string>(targetNodes.Select(n => n.Signature));
            commonSignatures.IntersectWith(targetSignatures);

            var nbNoMatch = matching.Count(m =>
                commonSignatures.Contains(m.Source?.Signature ?? m.Target.Signature)
                && (m.Source == null || m.Target == null)
            );
            var nbMismatch = matching.Count(m => m.Source != null && m.Target != null  && m.Source.Signature != m.Target.Signature);
            var goodMatches = matching.Count(m => m.Source != null && m.Target != null  && m.Source.Signature == m.Target.Signature);
            
            var signaturesMatching = matching.Select(m => (m.Source?.Signature, m.Target?.Signature));

            return new Result
            {
                NbMismatch           = nbMismatch,
                SignatureMatching    = signaturesMatching,
                NbNoMatchUnjustified = nbNoMatch,
                NbNoMatch            = matching.Count(m => m.Source == null || m.Target == null),
                ComputationTime      = computationTime,
                Total                = minCount,
                MaxGoodMatches = commonSignatures.Count,
                GoodMatches = goodMatches
            };
        }
    }
}