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
            "sftm" => new SftmTreeMatcher(Settings.SFTMParameters()),
            "rted" => new RtedTreeMatcher(Settings.RTEDParameters),
            _      => throw new Exception("Unknown Matcher")
        };

        [HttpGet]
        public async Task<ActionResult<TreeVizResult>> Get(int index, string matcherName)
        {
            var matcher = GetMatcher(matcherName);
            var (key, source, target) = BolzanoImporter.GetBolzanoTrees().Skip(index).First();
            var resultMatching = await matcher.MatchTrees(source, target);
            return await tree_matching_csharp.Visualization.Utils.Utils.ToTreeViz(resultMatching, source, target);
        }
    }
}