using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using AngleSharp;
using AngleSharp.Dom;
using HtmlAgilityPack;
using MoreLinq.Extensions;
using Nest;

namespace tree_matching_csharp
{
    public static class DOM
    {
        public const string AttributeName = "signature";
        
        public const int    MaxTokenPerValue = 8;

        private static IEnumerable<string> TokenizeValue(string value)
        {
            var valueTokens = Regex.Replace(value, @"\W+", " ").Split(" ")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            if (!valueTokens.Any())
                return new List<string>();
            
            var hash = value;
            if (valueTokens.Count > 1)
                valueTokens.Add(hash);
            
            return valueTokens.Count < MaxTokenPerValue ? valueTokens : new List<string> {hash};
        }
        private static IEnumerable<string> TokenizeAttributes(IAttr attr)
        {
            if (attr.Name == AttributeName || attr.Name.StartsWith("data"))
                return new string[] { };

            var tokens = new List<string> {attr.Name};
            if (string.IsNullOrWhiteSpace(attr.Value)) return tokens;

            tokens.AddRange(TokenizeValue(attr.Value).Select((v => $"{attr.Name}:{v}")));
            
            return tokens;
        }

        private static string GetOwnContent(this INode el)
        {
            var values = el.ChildNodes
                .Where(n => n.NodeType == NodeType.Text)
                .Select(n => n.NodeValue
                    .Replace("\n", "")
                    .Replace("\t", "")
                    .Replace("\r", "")
                    .ToLower()
                    .Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n));

            return string.Join(' ', values);
        }
        
        private static List<string> TokenizeNode(IElement el)
        {
            var tokens = new List<string> {el.TagName};
            foreach (var attr in el.Attributes)
                tokens.AddRange(TokenizeAttributes(attr).Select(v => $"{el.TagName}:{v}"));

            // var content = el.GetOwnContent();
            // var contentTokens = TokenizeValue(content).Select(v => $"{el.TagName}:content:{v}");
            // tokens.AddRange(contentTokens);
            
            return tokens;
        }

        public static async Task<IDocument> WebpageToDocument(string webpage)
        {
            var config  = Configuration.Default;
            var context = BrowsingContext.New(config);

            return await context.OpenAsync(req => req.Content(webpage));
        }

        private static string GetNewXPath(IElement el, IParentNode? parent, string partialXPath)
        {
            var siblingsWithSameTag = parent?.Children?.Where(e => e.TagName == el.TagName);
            var indexCurrentElement = siblingsWithSameTag?.Index(el);
            var brackets = indexCurrentElement != null && indexCurrentElement != -1
                ? $"[{indexCurrentElement}]"
                : "";

            return partialXPath + $"/{el.TagName}" + brackets;
        }

        public static IEnumerable<Node> DomToTree(IDocument doc)
        {
            var nodes = new List<Node>();

            Node Copy(IElement el, IParentNode parent, Node parentNode, Node leftSibling, string partialXPath)
            {
                var newXPath = GetNewXPath(el, parent, partialXPath);
                var tokenizedNode = TokenizeNode(el);
                tokenizedNode.Add(newXPath);
                var node = new Node
                {
                    Value     = tokenizedNode,
                    Signature = el.Attributes.GetNamedItem(AttributeName)?.Value,
                    Parent    = parentNode,
                    LeftSibling = leftSibling
                };
                nodes.Add(node);

                Node prevChild = null;
                foreach (var child in el.Children)
                    prevChild = Copy(child, el, node, prevChild, newXPath);

                return node;
            }

            Copy(doc.Body, null, null, null, "");

            return nodes;
        }

        public static IEnumerable<HtmlNode> GetNodesInPostOrder(this HtmlNode root)
        {
            var nodes = new List<HtmlNode>();
            root.PostOrderTraversal(n => {nodes.Add(n);});

            return nodes;
        }

        public static void PostOrderTraversal(this HtmlNode root, Action<HtmlNode> f)
        {
            foreach (var child in root.ChildNodes)
                child.PostOrderTraversal(f);

            f(root);
        }
        
        public static IEnumerable<XmlNode> GetNodesInPostOrder(this XmlNode root)
        {
            var nodes = new List<XmlNode>();
            root.PostOrderTraversal(n => {nodes.Add(n);});

            return nodes;
        }
        
        public static void PostOrderTraversal(this XmlNode root, Action<XmlNode> f)
        {
            foreach (XmlNode child in root.ChildNodes)
                PostOrderTraversal(child, f);

            f(root);
        }

        public static async Task<IEnumerable<Node>> WebpageToTree(string source)
        {
            var document = await WebpageToDocument(source);

            return DomToTree(document);
        }
    }
}