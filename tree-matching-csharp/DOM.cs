using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Nest;

namespace tree_matching_csharp
{
    public static class DOM
    {
        private const string AttributeName = "signature";

        private static IEnumerable<string> TokenizeAttributes(IAttr attr)
        {
            if (attr.Name == AttributeName)
                return new string[] {};
            
            var value = attr.Value.Split(" ");
            value.Append(attr.Name);
            
            return value;
        }

        private static IList<string> TokenizeNode(IElement el)
        {
            var tokens = new List<string> {el.TagName};
            foreach (var attr in el.Attributes)
                tokens.AddRange(TokenizeAttributes(attr));

            return tokens;
        }

        private static async Task<IDocument> WebpageToDocument(string webpage)
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

            void Copy(IElement el, IParentNode parent, Node parentNode, string partialXPath)
            {
                var newXPath = GetNewXPath(el, parent, partialXPath);
                var tokenizedNode = TokenizeNode(el);
                tokenizedNode.Add(newXPath);
                var node = new Node
                {
                    Value     = tokenizedNode,
                    Signature = el.Attributes.GetNamedItem(AttributeName)?.Value,
                    Parent    = parentNode
                };
                nodes.Add(node);

                foreach (var child in el.Children)
                    Copy(child, el, node, newXPath);
            }

            Copy(doc.Body, null, null, "");

            return nodes;
        }

        public static async Task<IEnumerable<Node>> WebpageToTree(string source)
        {
            var document = await WebpageToDocument(source);

            return DomToTree(document);
        }
    }
}