using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using HtmlAgilityPack;
using MoreLinq;
using Newtonsoft.Json;
using static System.String;

namespace tree_matching_csharp.Benchmark
{
    public class XyDiffMatcher : IWebsiteMatcher
    {
        private HttpClient _client;
        private const string ContainerName = "fake";

        private class Response
        {
            public IEnumerable<IEnumerable<int>> mapping { get; set; }
            public float                         time    { get; set; }
        }

        private const string Signature = "signature";

        public XyDiffMatcher()
        {
            _client = new HttpClient {Timeout = Timeout.InfiniteTimeSpan};
        }

        public static XmlNode WebpageToXhtml(string html)
        {
            var doc = new HtmlDocument {OptionWriteEmptyNodes = true, OptionOutputAsXml = true};
            doc.LoadHtml(html);

            var outer = doc.DocumentNode.SelectSingleNode("//body").OuterHtml;

            var xml = new XmlDocument{PreserveWhitespace = false};
            xml.LoadXml(outer);

            var container = xml.CreateNode(XmlNodeType.Element, ContainerName, "");

            container.AppendChild(xml.DocumentElement);
            // RemoveIgnorableWhitespace(container);
            return container;
        }

        // The parser does it better, but I have to mimic the behavior of the xydiff parser so that our post-order-id will be the same
        public static void RemoveIgnorableWhitespace(XmlNode n)
        {
            if (!n.HasChildNodes) return;
            var child = n.FirstChild;
            while (child != null)
            {
                if (child.HasChildNodes)
                    RemoveIgnorableWhitespace(child);

                var nextChild = child.NextSibling;
                if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.Whitespace)
                {
                    var childContent = child.Value;
                    if (IsNullOrWhiteSpace(childContent))
                    {
                        if (child.NextSibling != null || child.PreviousSibling != null)
                        {
                            n.RemoveChild(child);
                        }
                    }
                }

                child = nextChild;
            }
        }

        public async Task<WebsiteMatcher.Result?> MatchWebsites(string source, string target)
        {
            var sourceDoc = WebpageToXhtml(source);
            var targetDoc = WebpageToXhtml(target);

            var sourceSignatures = HandleSignatures(sourceDoc);
            var targetSignatures = HandleSignatures(targetDoc);

            var encodedSource = HttpUtility.HtmlEncode(sourceDoc.OuterXml);
            var encodedTarget = HttpUtility.HtmlEncode(targetDoc.OuterXml);
            var nodes = sourceDoc.GetNodesInPostOrder().Select(n => $"{n.NodeType}: {n.Name}: {n.Value}").ToList();

            var response = await XyDiff(encodedSource, encodedTarget);

            var sourceIdToSignature = sourceSignatures.ToDictionary(t => t.Item2, t => t.Item1);
            var targetIdToSignature = targetSignatures.ToDictionary(t => t.Item2, t => t.Item1);

            var mappingSize   = response.mapping.Count();
            var signatureSize = sourceIdToSignature.Keys.Count();


            if (mappingSize > signatureSize)
                throw new Exception("The mapping size shouldnt be bigger than the signature size");

            var matching = response.mapping
                .Select(m =>
                {
                    var match = m.ToList();
                    return new {sourceSignature = sourceIdToSignature[match[0]], targetSignature = targetIdToSignature[match[1]]};
                });

            var goodMatches = matching
                .Where(m => m.sourceSignature != null)
                .Where(m => m.sourceSignature == m.targetSignature);

            var mismatch = matching
                .Where(m => m.sourceSignature != null && m.targetSignature != null)
                .Where(m => m.sourceSignature != m.targetSignature);

            var noMatch = sourceIdToSignature.Values
                .Where(s => s != null)
                .Where(signature => !matching.Select(m => m.sourceSignature).Contains(signature));

            var noMatchUnjustified = noMatch
                .Where(signature => targetIdToSignature.Values.Contains(signature));
            
            var commonSignatures = new HashSet<string>(sourceIdToSignature.Values.Concat(targetIdToSignature.Values).Where(s => s != null));

            return new WebsiteMatcher.Result
            {
                Total                = Math.Min(sourceSignatures.Count(), targetSignatures.Count()),
                ComputationTime      = (long) response.time * 1000,
                GoodMatches          = goodMatches.Count(),
                NbMismatch           = mismatch.Count(),
                NbNoMatch            = noMatch.Count(),
                NbNoMatchUnjustified = noMatchUnjustified.Count(),
                MaxGoodMatches = commonSignatures.Count()
            };
        }

        private async Task<Response> XyDiff(string source, string target)
        {
            var input        = new {source = source, target = target};
            var contentInput = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            var response     = await _client.PostAsync(Settings.UrlXyDiff, contentInput);

            var responseContent = await response.Content.ReadAsStringAsync();
            var parsedResponse  = JsonConvert.DeserializeObject<Response>(responseContent);
            if (parsedResponse.mapping == null)
                throw new Exception("No Mapping");

            return parsedResponse;
        }

        private static IEnumerable<(string?, int)> HandleSignatures(XmlNode doc)
        {
            var acceptedNodes     = doc.GetNodesInPostOrder().Where(n => n.Name != ContainerName);
            var totalNodes        = acceptedNodes.Count();
            var signatureId       = acceptedNodes.Select((node, id) => (GetSignature(node), id + 1)).ToList();

            acceptedNodes
                .Where(n => n.NodeType == XmlNodeType.Element)
                .ForEach(el => { el.Attributes.RemoveNamedItem(Signature); });

            return signatureId;
        }


        private static string? GetSignature(XmlNode node) => node.Attributes?.GetNamedItem(Signature)?.Value;
    }
}