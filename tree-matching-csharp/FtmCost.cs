using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Common;
using MoreLinq.Extensions;

namespace tree_matching_csharp
{
    public class FtmCost
    {
        public class Cost
        {
            public Cost(int noMatch = 0, double relabel = 0, int ancestry = 0, int sibling = 0)
            {
                Relabel  = relabel;
                Ancestry = ancestry;
                Sibling  = sibling;
                NoMatch  = noMatch;
            }

            public double Total => Relabel + Ancestry + Sibling + NoMatch;

            public double Relabel   { get; set; }
            public double Ancestry  { get; set; }
            public double Sibling   { get; set; }
            public int    NoMatch   { get; set; }
            public bool   IsCorrect => NoMatch < 1 && Relabel + Ancestry + Sibling < 0.3;
        }

        private readonly IEnumerable<Edge>               _matching;
        private readonly Dictionary<Node, Node>          _matchingDic;
        private readonly Dictionary<Node, HashSet<Node>> _childrenDic;

        public FtmCost(List<Edge> matching)
        {
            _matching    = matching;
            _matchingDic = ComputeMatchingDic(matching);

            var nodes = ComputeNodes(matching);
            _childrenDic = ComputeChildrenDic(nodes);
        }


        public Cost ComputeCost()
        {
            foreach (var edge in _matching)
            {
                var cost = ComputeCost(edge);
                edge.FtmCost = cost;
            }

            return new Cost
            {
                Ancestry = _matching.Select(c => c.FtmCost.Ancestry).Sum(),
                Relabel  = _matching.Select(c => c.FtmCost.Relabel).Sum(),
                Sibling  = _matching.Select(c => c.FtmCost.Sibling).Sum(),
                NoMatch  = _matching.Select(c => c.FtmCost.NoMatch).Sum(),
            };
        }

        private Cost ComputeCost(Edge matching)
        {
            if (matching.Source == null || matching.Target == null)
                return new Cost(1);

            var relabelCost = RelabelCost(matching.Source, matching.Target);
            var ancestryCost =
                NbViolations(
                    _childrenDic.GetOrDefault(matching.Source, new HashSet<Node>()),
                    _childrenDic.GetOrDefault(matching.Target, new HashSet<Node>()))
                +
                NbViolations(
                    _childrenDic.GetOrDefault(matching.Target, new HashSet<Node>()),
                    _childrenDic.GetOrDefault(matching.Source, new HashSet<Node>()));

            var siblingCost =
                NbViolations(GetSiblings(matching.Source), GetSiblings(matching.Target))
                +
                NbViolations(GetSiblings(matching.Target), GetSiblings(matching.Source));

            var totalChildren = matching.Source.Children.Count + matching.Target.Children.Count;
            var totalSiblings = (matching.Source.Parent?.Children.Count ?? 0) + (matching.Target.Parent?.Children.Count ?? 0);

            var ancestry = totalChildren == 0 ? 0 : ancestryCost / (double) totalChildren;
            var siblings = totalSiblings == 0 ? 0 : siblingCost / (double) totalSiblings;

            return new Cost
            {
                Ancestry = ancestry / 3,
                Relabel  = relabelCost / 3.0,
                Sibling  = siblings / 3,
                NoMatch  = 0
            };
        }

        private static HashSet<Node> ComputeNodes(IEnumerable<Edge> matching) =>
            new HashSet<Node>(matching.SelectMany(m => new[] {m.Source, m.Target}).Where(n => n != null));

        private IEnumerable<Node> GetSiblings(Node node) =>
            (node.Parent == null ? new HashSet<Node>() : _childrenDic.GetOrDefault(node.Parent, new HashSet<Node>()).Except(new[] {node})).ToList();

        private static Dictionary<Node, HashSet<Node>> ComputeChildrenDic(HashSet<Node> nodes)
        {
            var result = new Dictionary<Node, HashSet<Node>>();
            nodes.ForEach(n =>
            {
                if (n.Parent == null)
                    return;
                if (result.ContainsKey(n.Parent))
                    result[n.Parent].Add(n);
                else
                    result.Add(n.Parent, new HashSet<Node> {n});
            });

            return result;
        }

        private static Dictionary<Node, Node> ComputeMatchingDic(IEnumerable<Edge> matching)
        {
            var result = new Dictionary<Node, Node>();
            matching
                .Where(edge => edge.Source != null && edge.Target != null)
                .ForEach(edge =>
                {
                    result[edge.Source] = edge.Target;
                    result[edge.Target] = edge.Source;
                });

            return result;
        }

        private static double RelabelCost(Node source, Node target)
        {
            var intersectionSize = source.Value
                .Count(s => target.Value.Contains(s));

            var maxLength = Math.Max(source.Value.Count, target.Value.Count);

            return 1 - intersectionSize / (double) maxLength;
        }

        // We want to know how many of nodesSource's matched nodes are in nodesTarget
        // Use case: 1. (nodesSource = childrenSource, nodesTarget = childrenTarget) 2. (nodesSource = SibilngsSource, siblingsTarget)
        private int NbViolations(IEnumerable<Node> nodesSource, IEnumerable<Node> nodesTarget)
        {
            var a = nodesSource.Select(n => _matchingDic.GetOrDefault(n, null));
            var b = a.Where(n => n != null);
            var c = b.Except(nodesTarget);
            var d = c.Count();
            return d;
        }
    }
}