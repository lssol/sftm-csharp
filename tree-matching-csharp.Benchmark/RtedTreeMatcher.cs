﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Common;
using MoreLinq;
using Newtonsoft.Json;

namespace tree_matching_csharp.Benchmark
{
    public class RtedTreeMatcher : ITreeMatcher
    {
        private readonly Parameters _parameters;
        private readonly HttpClient _client;

        public enum LabelCostFunction
        {
            String,
            Default
        }
        public class Parameters
        {
            public double InsertionCost { get; set; }
            public double DeletionCost  { get; set; }
            public double RelabelCost   { get; set; }
            public LabelCostFunction LabelCostFunction { get; set; }
        }

        private class ResponseApi
        {
            public class Match
            {
                public int Id1 { get; set; }
                public int Id2 { get; set; }
            }

            public int                Time { get; set; }
            public double             Distance        { get; set; }
            public IEnumerable<Match> Matching        { get; set; }
        }

        public RtedTreeMatcher(Parameters parameters)
        {
            _parameters = parameters;
            _client = new HttpClient {Timeout = Timeout.InfiniteTimeSpan};
        }

        public async Task<TreeMatcherResponse?> MatchTrees(IEnumerable<Node> sourceNodes, IEnumerable<Node> targetNodes)
        {
            sourceNodes.ComputeChildren();
            targetNodes.ComputeChildren();

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
            var url = _parameters.LabelCostFunction switch
            {
                LabelCostFunction.String  => Settings.UrlRTEDString,
                LabelCostFunction.Default => Settings.UrlRTEDDefault,
                _ => throw new Exception("Unsupported label cost function")
            };
            var response        = await _client.PostAsync(url, contentInput);
            var responseContent = await response.Content.ReadAsStringAsync();
            var parsedResponse  = JsonConvert.DeserializeObject<ResponseApi>(responseContent);
            if (parsedResponse.Matching == null)
                return null;
            
            return new TreeMatcherResponse
            {
                ComputationTime = parsedResponse.Time,
                Edges = parsedResponse.Matching.Select(m => new Edge
                {
                    Source = sourceIds.GetOrDefault(m.Id1, null),
                    Target = targetIds.GetOrDefault(m.Id2, null)
                }).Where(e => e.Source != null || e.Target != null)
            };
        }

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