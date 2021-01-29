using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using MoreLinq.Extensions;
using tree_matching_csharp.Benchmark.BracketParser;

namespace tree_matching_csharp.Benchmark
{
    public static class BolzanoImporter
    {
        public static IEnumerable<(string, IEnumerable<Node>, IEnumerable<Node>)> GetBolzanoTrees()
        {
            var lTrees = GetTreesFromFile("data/bolzano/L.trees");
            var rTrees = GetTreesFromFile("data/bolzano/R.trees");

            var commonKeys = lTrees.Keys.Intersect(rTrees.Keys);

            return commonKeys
                .Select(key => (key, BracketToNodeList(lTrees[key]), BracketToNodeList(rTrees[key])));
        }

        private static Dictionary<string, BracketTree> GetTreesFromFile(string path)
        {
            var parser = new PegParser.BracketParser();
            var lines  = File.ReadLines(path);

            return lines
                .Where(l => !l.StartsWith('#'))
                .Select(l => l.Split(":"))
                .Where(l => l.Length == 2)
                .ToDictionary(l => l[0], l => parser.Parse(l[1]));
        }

        private static IEnumerable<Node> BracketToNodeList(BracketTree bracketTree)
        {
            var nodeList = new List<Node>();

            Node ComputeNodes(BracketTree root, Node parent, Node leftSibling)
            {
                var node = new Node
                {
                    Value       = parent == null ? new List<string> {"ROOT"} : new List<string> {"_" + root.Label},
                    Parent      = parent,
                    LeftSibling = leftSibling
                };
                nodeList.Add(node);
                Node previousNode = null;
                foreach (var child in root.Children)
                    previousNode = ComputeNodes(child, node, previousNode);

                return node;
            }

            ComputeNodes(bracketTree, null, null);
            return nodeList;
        }
    }
}