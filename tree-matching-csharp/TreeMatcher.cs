using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tree_matching_csharp
{
    public class TreeMatcher
    {
        public class Parameters
        {
            public float[]               WeightsPropagation   { get; set; }
            public Metropolis.Parameters MetropolisParameters { get; set; }
            public int                   LimitNeighbors       { get; set; }
        }

        private readonly Parameters _parameters;

        public TreeMatcher(Parameters parameters)
        {
            _parameters = parameters;
        }

        private static bool IsSignaturePresent(IEnumerable<Node> nodes)
        {
            return nodes.All(n => n.Signature != null);
        }

        public async Task<Dictionary<string, string>> MatchWebsites(string source, string target)
        {
            var indexer = new Indexer();
            var sourceNodes = await DOM.WebpageToTree(source);
            var targetNodes = await DOM.WebpageToTree(target);
            
            if (!IsSignaturePresent(sourceNodes) || !IsSignaturePresent(targetNodes))
                throw new Exception("The web documents are expected to contain signature attributes");

            var neighbors = await indexer.FindNeighbors(sourceNodes, targetNodes);
            SimilarityPropagation.PropagateSimilarity(neighbors, _parameters.WeightsPropagation);

            var metropolis = new Metropolis(_parameters.MetropolisParameters, Utils.NeighborsToEdges(neighbors), sourceNodes.Count() + targetNodes.Count());
            var matching = metropolis.Run();

            var signaturesMatching = matching.ToDictionary(m => m.Source.Signature, m => m.Target.Signature);

            return signaturesMatching;
        }
    }
}