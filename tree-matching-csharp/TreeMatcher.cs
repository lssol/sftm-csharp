using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Common;

namespace tree_matching_csharp
{
    public class TreeMatcher
    {
        public class Parameters
        {
            public float[]               WeightsPropagation   { get; set; }
            public Metropolis.Parameters MetropolisParameters { get; set; }
            public float                 NoMatchCost          { get; set; }
            public int                   LimitNeighbors       { get; set; }
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

        public async Task<IEnumerable<(string, string)>> MatchWebsites(string source, string target)
        {
            var indexer     = new Indexer();
            var sourceNodes = await DOM.WebpageToTree(source);
            var targetNodes = await DOM.WebpageToTree(target);

            if (!IsSignaturePresent(sourceNodes) || !IsSignaturePresent(targetNodes))
                throw new Exception("The web documents are expected to contain signature attributes");

            var neighbors = await indexer.FindNeighbors(sourceNodes, targetNodes);
            SimilarityPropagation.PropagateSimilarity(neighbors, _param.WeightsPropagation);

            var edges      = Utils.NeighborsToEdges(neighbors).Concat(GetNoMatchEdges(sourceNodes, targetNodes));
            var metropolis = new Metropolis(_param.MetropolisParameters, edges, sourceNodes.Count() + targetNodes.Count());
            var matching   = metropolis.Run();

            var signaturesMatching = matching.Select(m => (m.Source?.Signature, m.Target?.Signature));

            return signaturesMatching;
        }
    }
}