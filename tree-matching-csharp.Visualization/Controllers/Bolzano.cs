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
        private IAsyncEnumerator<(IEnumerable<Node> source, IEnumerable<Node> target, SimulationResultBracket)> _mutationGeneratorSFTM;
        private IAsyncEnumerator<(IEnumerable<Node> source, IEnumerable<Node> target, SimulationResultBracket)> _mutationGeneratorRTED;

        public Bolzano(ILogger<Bolzano> logger)
        {
            var sftm = new SftmTreeMatcher(Settings.SFTMParameters);
            var rted = new RtedTreeMatcher(Settings.RTEDParameters);
            
            _mutationGeneratorSFTM = MutationBenchmark.RunBracket("SFTM", sftm, "bolzano").GetAsyncEnumerator();
            _mutationGeneratorRTED = MutationBenchmark.RunBracket("RTED", rted, "bolzano").GetAsyncEnumerator();
            
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<TreeVizResult>> Get()
        {
            await _mutationGeneratorSFTM.MoveNextAsync();
            var (source, target, res) = _mutationGeneratorSFTM.Current;
            return new TreeVizResult
            {
                Tree1 = source.Select(n => n.ToCyto()),
                Tree2 = target.Select(n => n.ToCyto()),
                Matching = new Matching
                {
                    Time = res.ComputationTime,
                    Cost = res.FTMRelativeCost,
                    Matches = res.Matching.Select(edge => new Matching.Match
                    {
                        Id1 = edge.Source?.Id.ToString(),
                        Id2 = edge.Target?.Id.ToString(),
                        Cost = edge.FtmCost
                    })
                }
            };
        }
    }
}