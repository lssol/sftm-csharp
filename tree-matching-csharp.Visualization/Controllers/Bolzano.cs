using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using tree_matching_csharp.Benchmark;
using tree_matching_csharp.Visualization.Models;
using tree_matching_csharp.Visualization.Utils;

namespace tree_matching_csharp.Visualization.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Bolzano : ControllerBase
    {
        private readonly ILogger<Bolzano> _logger;

        public Bolzano(ILogger<Bolzano> logger)
        {
            _logger = logger;
        }

        private ITreeMatcher GetMatcher(string matcher) => matcher switch
        {
            "sftm" => new SftmTreeMatcher(Settings.SFTMParameters),
            "rted" => new RtedTreeMatcher(Settings.RTEDParameters),
            _      => throw new Exception("Unknown Matcher")
        };

        [HttpGet]
        public async Task<ActionResult<TreeVizResult>> Get(int index, string matcherName)
        {
            var matcher = GetMatcher(matcherName);
            var (key, source, target) = BolzanoImporter.GetBolzanoTrees().Skip(index).First();
            var resultMatching = await matcher.MatchTrees(source, target);
            var maxTotal       = Math.Max(source.Count(), target.Count());
            var edges = resultMatching.Edges.ToList();
            var ftmCost        = new FtmCost(edges).ComputeCost();
            var ftmRelativeCost = (ftmCost.Ancestry + ftmCost.Relabel + ftmCost.Sibling + ftmCost.NoMatch) / maxTotal;
            return new TreeVizResult
            {
                Tree1 = source.Select(n => n.ToCyto()),
                Tree2 = target.Select(n => n.ToCyto()),
                Matching = new Matching
                {
                    Time = resultMatching.ComputationTime,
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