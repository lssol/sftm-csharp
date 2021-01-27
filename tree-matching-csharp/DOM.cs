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
using AngleSharp.Text;
using HtmlAgilityPack;
using MoreLinq.Extensions;
using Nest;

namespace tree_matching_csharp
{
    public static class DOM
    {
        public const string AttributeName    = "signature";
        public const int    MaxTokenPerValue = 8;

        private static IEnumerable<ulong> TokenizeValue(string value)
        {
            var valueTokens = Regex.Replace(value, @"\W+", " ").Split(" ")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(Utils.Hash)
                .ToList();
            
            var hash = value.Hash();
            valueTokens.Add(hash);
            return valueTokens.Count < MaxTokenPerValue ? valueTokens : new List<ulong> {hash};
        }
        
        private static IEnumerable<ulong> TokenizeAttributes(IAttr attr)
        {
            if (attr.Name == AttributeName)
                return new List<ulong>();

            var tokens = new List<ulong> {attr.Name.Hash()};
            if (string.IsNullOrWhiteSpace(attr.Value)) return tokens;

            tokens.AddRange(TokenizeValue(attr.Value));
            
            return tokens;
        }

        private static List<ulong> TokenizeNode(IElement el)
        {
            var tokens = new List<ulong> {el.TagName.Hash()};
            foreach (var attr in el.Attributes)
                tokens.AddRange(TokenizeAttributes(attr));

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
                tokenizedNode.Add(newXPath.Hash());
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