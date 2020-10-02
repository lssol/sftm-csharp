using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace tree_matching_csharp.Benchmark
{
    public class WebsiteMatcher : IWebsiteMatcher
    {
        public class Result
        {
            public IEnumerable<(string?, string?)>? SignatureMatching    { get; set; }
            public IEnumerable<Edge>?             Matching             { get; set; }
            public int                           NbMismatch           { get; set; }
            public int                           NbNoMatch            { get; set; }
            public int                           NbNoMatchUnjustified { get; set; }
            public long                          ComputationTime      { get; set; }
            public int                           Total                { get; set; }
            public int                           MaxGoodMatches       { get; set; }
            public int                           GoodMatches          { get; set; }
        }

        private readonly ITreeMatcher _matcher;

        public WebsiteMatcher(ITreeMatcher matcher)
        {
            _matcher = matcher;
        }

        public async Task<Result?> MatchWebsites(string source, string target)
        {
            var sourceNodes = await DOM.WebpageToTree(source);
            var targetNodes = await DOM.WebpageToTree(target);

            var minCount = Math.Min(sourceNodes.Count(), targetNodes.Count());

            var matching = await _matcher.MatchTrees(sourceNodes, targetNodes);
            if (matching == null)
                return null;
            
            var computationTime = matching.ComputationTime;

            if (!IsSignaturePresent(sourceNodes) || !IsSignaturePresent(targetNodes))
                throw new Exception("The web documents are expected to contain signature attributes");

            var commonSignatures = new HashSet<string>(sourceNodes.Select(n => n.Signature));
            var targetSignatures = new HashSet<string>(targetNodes.Select(n => n.Signature));
            commonSignatures.IntersectWith(targetSignatures);

            var nbNoMatch = matching.Edges.Count(m =>
                commonSignatures.Contains((m.Source?.Signature ?? m.Target?.Signature)!)
                && (m.Source == null || m.Target == null)
            );
            var nbMismatch  = matching.Edges.Count(m => m.Source != null && m.Target != null && m.Source.Signature != m.Target.Signature);
            var goodMatches = matching.Edges.Count(m => m.Source != null && m.Target != null && m.Source.Signature == m.Target.Signature);

            var signaturesMatching = matching.Edges?.Select(m => (m.Source?.Signature, m.Target?.Signature));

            return new Result
            {
                NbMismatch           = nbMismatch,
                SignatureMatching    = signaturesMatching,
                NbNoMatchUnjustified = nbNoMatch,
                NbNoMatch            = matching.Edges.Count(m => m.Source == null || m.Target == null),
                ComputationTime      = computationTime,
                Total                = minCount,
                MaxGoodMatches       = commonSignatures.Count,
                GoodMatches          = goodMatches,
                Matching             = matching.Edges
            };
        }

        private static bool IsSignaturePresent(IEnumerable<Node> nodes)
        {
            var nullSignatures = nodes.Where(n => n.Signature == null);

            return nullSignatures.Count() <= 0.2 * nodes.Count();
        }
    }
}