using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

namespace tree_matching_csharp
{
    public static class DOM
    {
        private const string AttributeName = "signature";

        private static string StringifyAttribute(IAttr attr) =>
            attr.Name == AttributeName ? "" : $"{attr.Name} {attr.Value} {attr.Value}";

        private static string StringifyNode(IElement el) =>
            $"{el.TagName} {string.Join(" ", el.Attributes.Select(StringifyAttribute))}";

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

        public static Tree DomToTree(IDocument doc)
        {
            var tree = new Tree();

            void Copy(IElement el, IParentNode parent, Node parentNode, string partialXPath)
            {
                var newXPath = GetNewXPath(el, parent, partialXPath);
                var node = new Node
                {
                    Value = StringifyNode(el), 
                    Signature = el.Attributes.GetNamedItem(AttributeName)?.Value,
                    XPath = newXPath
                };
                tree.Nodes.Add(node);
                
                if (parent != null)
                    tree.Edges.Add(new Edge
                    {
                        Cost  = null,
                        Node1 = parentNode, 
                        Node2 = node
                    });
                
                foreach (var child in el.Children)
                    Copy(child, el, node, newXPath);
            }
            
            Copy(doc.Body, null, null, "");
            
            return tree;
        }

        public static async Task<Tree> WebpageToTree(string source)
        {
            var document = await WebpageToDocument(source);

            return DomToTree(document);
        }
    }
}