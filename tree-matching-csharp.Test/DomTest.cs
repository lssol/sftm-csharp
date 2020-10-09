using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using AngleSharp.Dom;
using Elasticsearch.Net;
using HtmlAgilityPack;
using MoreLinq;
using System.Xml.Linq;
using NUnit.Framework;
using Pegasus.Common;
using tree_matching_csharp.Benchmark;

namespace tree_matching_csharp.Test
{
    public class DomTests
    {
        public static string SimpleWebpage = @"
            <!DOCTYPE html>

            <html>
                <head></head>
                <body>
                    <!-- A comment -->
                    <h1>A title</h1>
                    <div>
                        <p>p1</p>
                        <p>p2</p>
                    </div>
                </body>
            </html>
        ";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task WebpageToTree()
        {
            var nodes = await DOM.WebpageToTree(SimpleWebpage);
            Assert.AreEqual(nodes.Count(), 5);
        }


        [Test]
        public void GetText()
        {
            var doc = new HtmlDocument {OptionOutputAsXml = true, OptionAutoCloseOnEnd = true};
            doc.LoadHtml(SimpleWebpage);
            var outer = doc.DocumentNode.SelectSingleNode("//body").OuterHtml;

            var xml = new XmlDocument();
            xml.LoadXml(outer);
            Console.WriteLine("Extracted xml (from body)");

            var nodes = xml.DocumentElement.GetNodesInPostOrder();
            nodes.ForEach(n => Console.WriteLine($"{n.NodeType}: {n.Name}"));
            Assert.AreEqual(nodes.Count(), 9);
        }

        [Test]
        public void HtmlToXml()
        {
            var r = XyDiffMatcher.WebpageToXhtml(SimpleWebpage);
            Console.WriteLine(r.OuterXml);
            Console.WriteLine("The number of nodes is");
            Console.WriteLine(r.GetNodesInPostOrder().Count());
        }
    }
}