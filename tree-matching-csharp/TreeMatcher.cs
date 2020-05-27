using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
            public IEnumerable<(string, string)> SignatureMatching { get; set; }
            public int                           NbMismatch        { get; set; }
            public int                           NbNoMatch         { get; set; }
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

        public async Task<Result> MatchWebsites(string source, string target)
        {
            var watch   = new Stopwatch();

            watch.Restart();
            var sourceNodes = await DOM.WebpageToTree(source);
            var targetNodes = await DOM.WebpageToTree(target);
            watch.Stop();
            Console.WriteLine($"Creating trees took: {watch.ElapsedMilliseconds}");
            Console.WriteLine($"The trees have {sourceNodes.Count()} and {targetNodes.Count()} nodes");
            
            var indexer = new InMemoryIndexer(_param.LimitNeighbors, _param.MaxTokenAppearance(sourceNodes.Count()));

            if (!IsSignaturePresent(sourceNodes) || !IsSignaturePresent(targetNodes))
                throw new Exception("The web documents are expected to contain signature attributes");

            var neighbors = indexer.FindNeighbors(sourceNodes, targetNodes);
            neighbors = SimilarityPropagation.PropagateSimilarity(neighbors, _param.PropagationParameters);

            var noMatchEdges = GetNoMatchEdges(sourceNodes, targetNodes);
            var edges        = neighbors.GetEdges().Concat(noMatchEdges);
            var metropolis   = new Metropolis(_param.MetropolisParameters, edges, sourceNodes.Count() + targetNodes.Count());

            watch.Restart();
            var matching = metropolis.Run();
            watch.Stop();
            Console.WriteLine($"Metropolis took: {watch.ElapsedMilliseconds}");

            var sourceSignatures = new HashSet<string>(sourceNodes.Select(n => n.Signature));
            var targetSignatures = new HashSet<string>(targetNodes.Select(n => n.Signature));

            var nbNoMatch = matching.Count(m =>
                (m.Source != null && m.Target == null    && targetSignatures.Contains(m.Source.Signature))
                || (m.Source == null && m.Target != null && sourceSignatures.Contains(m.Target.Signature))
            );
            var nbMismatch = matching.Count(m =>
                m.Source    != null
                && m.Target != null
                && targetSignatures.Contains(m.Source.Signature)
                && m.Source.Signature != m.Target.Signature);

            var signaturesMatching = matching.Select(m => (m.Source?.Signature, m.Target?.Signature));

            return new Result
            {
                NbMismatch        = nbMismatch,
                SignatureMatching = signaturesMatching,
                NbNoMatch         = nbNoMatch
            };
        }
    }
}