using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using tree_matching_csharp.Visualization.Models;

namespace tree_matching_csharp.Visualization.Utils
{
    public static class Utils
    {
        public static CytoElementDef ToCyto(this Node node)
        {
            return new CytoElementDef
            {
                Group = "nodes",
                Data = new CytoElementDef.DataClass
                {
                    Id = node.Id.ToString(),
                    // Label = string.Join(" ", node.Value.Where(token => !token.StartsWith("#"))),
                    Parent = node.Parent?.Id.ToString(),
                    Value = node.Value
                }
            };
        }
        
        public static async Task<TreeVizResult> ToTreeViz(TreeMatcherResponse matching, IEnumerable<Node> source, IEnumerable<Node> target)
        {
            var maxTotal   = Math.Max(source.Count(), target.Count());
            var edges= matching.Edges.ToList();
            var ftmCost        = new FtmCost(edges).ComputeCost();
            var ftmRelativeCost = (ftmCost.Ancestry + ftmCost.Relabel + ftmCost.Sibling + ftmCost.NoMatch) / maxTotal;
            return new TreeVizResult
            {
                Tree1 = source.Select(n => n.ToCyto()),
                Tree2 = target.Select(n => n.ToCyto()),
                Matching = new Matching
                {
                    Time = matching.ComputationTime,
                    Cost = ftmCost,
                    RelativeCost = ftmRelativeCost,
                    Matches = edges.Select(edge => new Matching.Match
                    {
                        Id1  = edge.Source?.Id.ToString(),
                        Id2  = edge.Target?.Id.ToString(),
                        Cost = edge.FtmCost
                    })
                }
            };
        }
    }
}