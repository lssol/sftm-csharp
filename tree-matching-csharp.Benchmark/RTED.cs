using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json;

namespace tree_matching_csharp.Benchmark
{
    public class RTED : ITreeMatcher
    {
        private readonly Parameters _parameters;
        private readonly HttpClient _client;

        public class Parameters
        {
            public double InsertionCost { get; set; }
            public double DeletionCost  { get; set; }
            public double RelabelCost   { get; set; }
        }

        public class Response
        {
            public int               ComputationTime { get; set; }
            public double            Distance        { get; set; }
            public IEnumerable<Edge> Matching        { get; set; }
        }

        private class ResponseApi
        {
            public class Match
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }
            }

            public int                ComputationTime { get; set; }
            public double             Distance        { get; set; }
            public IEnumerable<Match> Matching        { get; set; }
        }

        public RTED(Parameters parameters)
        {
            _parameters = parameters;
            _client     = new HttpClient();
        }

        public async Task<TreeMatcherResponse> MatchTrees(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            ComputeChildren(sourceNodes);
            ComputeChildren(targetNodes);

            var sourceRoot = GetRoot(sourceNodes);
            var targetRoot = GetRoot(targetNodes);

            var sourceBracket = NodeToBracket(sourceRoot);
            var targetBracket = NodeToBracket(targetRoot);
            var sourceIds     = GetPostOrderIds(sourceRoot);
            var targetIds     = GetPostOrderIds(targetRoot);

            var input = new
            {
                t1            = sourceBracket,
                t2            = targetBracket,
                insertionCost = _parameters.InsertionCost,
                deletionCost  = _parameters.DeletionCost,
                relabelCost   = _parameters.RelabelCost,
            };
            var contentInput    = new StringContent(JsonConvert.SerializeObject(input));
            var response        = await _client.PostAsync(Settings.UrlRTED, contentInput);
            var responseContent = await response.Content.ReadAsStringAsync();
            var parsedResponse  = JsonConvert.DeserializeObject<ResponseApi>(responseContent);

            return new TreeMatcherResponse
            {
                ComputationTime = parsedResponse.ComputationTime,
                Edges = parsedResponse.Matching.Select(m => new Edge
                {
                    Source = sourceIds[m.Id1],
                    Target = targetIds[m.Id2],
                })
            };
        }

        private void ComputeChildren(IEnumerable<Node> nodes) => nodes.ForEach(n => n.Parent?.Children.Add(n));

        private Node GetRoot(IEnumerable<Node> nodes) => nodes.First(n => n.Parent == null);

        private Dictionary<int, Node> GetPostOrderIds(Node root)
        {
            var postOrderIds = new Dictionary<int, Node>();

            var i = 1;

            void ComputeIds(Node node)
            {
                node.Children.ForEach(ComputeIds);
                postOrderIds.Add(i, node);
                i++;
            }

            ComputeIds(root);

            return postOrderIds;
        }

        private string NodeToBracket(Node node)
        {
            var value = string.Join(' ', node.Value);
            if (node.Children.Count == 0)
                return $"{{{value}}}";
            var childrenValue = string.Join("", node.Children.Select(NodeToBracket));

            return $"{{{value}{childrenValue}}}";
        }
    }
}