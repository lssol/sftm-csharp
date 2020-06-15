using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;

namespace tree_matching_csharp.Benchmark
{
    public static class MutationBenchmark
    {
        public static async IAsyncEnumerable<SimulationResultMutation> RunMutation(string label, ITreeMatcher matcher)
        {
            var websiteMatcher = new WebsiteMatcher(matcher);
            var mongoRepo      = await MongoRepository.InitConnection();
            foreach (var (original, mutant) in mongoRepo.GetCouples())
            {
                if (await mongoRepo.MeasureAlreadyExists(label, mutant.Id.ToString()))
                {
                    Console.WriteLine("Skipped couple");
                    continue;
                }

                WebsiteMatcher.Result results;
                try
                {
                    results = await websiteMatcher.MatchWebsites(original.Content, mutant.Content);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                var mutationCouple = new MutationCouple {Mutant = mutant, Original = original};

                yield return results == null ? null : ToSimulationResult(results, mutationCouple, label);
            }
        }

        public static void CheckMatchingIsComplete(IEnumerable<Edge> edges, IEnumerable<Node> sources, IEnumerable<Node> targets)
        {
            var sourcesFromEdges = new HashSet<Node>(edges.Select(e => e.Source).Where(s => s != null));
            var targetsFromEdges = new HashSet<Node>(edges.Select(e => e.Target).Where(s => s != null));
            
            if (sources.Count() != sourcesFromEdges.Count)
                Console.WriteLine($"Different values, sources = {sources.Count()} and sourcesFromEdge = {sourcesFromEdges.Count()}");
            if (targets.Count() != targetsFromEdges.Count)
                Console.WriteLine($"Different values, targets = {targets.Count()} and targetsFromEdge = {targetsFromEdges.Count()}");
        }

        public static async IAsyncEnumerable<(IEnumerable<Node> source, IEnumerable<Node> target, SimulationResultBracket)> RunBracket(string label, ITreeMatcher matcher, string dataset)
        {
            foreach (var (key, source, target) in BolzanoImporter.GetBolzanoTrees())
            {
                TreeMatcherResponse resultMatching;
                try
                {
                    RtedTreeMatcher.ComputeChildren(source);
                    RtedTreeMatcher.ComputeChildren(target);
                    resultMatching = await matcher.MatchTrees(source, target);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
                if (resultMatching == null)
                    continue;

                var maxTotal = Math.Max(source.Count(), target.Count());
                var ftmCost = new FTMCost(resultMatching.Edges).ComputeCost();
                CheckMatchingIsComplete(resultMatching.Edges, source, target);
                var ftmRelativeCost = (ftmCost.Ancestry + ftmCost.Relabel + ftmCost.Sibling + ftmCost.NoMatch) / maxTotal;
                yield return (source, target, new SimulationResultBracket
                {
                    Dataset = dataset,
                    Id = key,
                    TotalSource = source.Count(),
                    TotalTarget = target.Count(),
                    ComputationTime = resultMatching.ComputationTime,
                    MatcherLabel = label,
                    NoMatch = resultMatching.Edges.Count(e => e.Source == null || e.Target == null),
                    FTMCost = ftmCost,
                    FTMRelativeCost = ftmRelativeCost
                });
            }
        }
        
        private static SimulationResultMutation ToSimulationResult(WebsiteMatcher.Result result, MutationCouple mutationCouple, string label)
        {
            var ftmCostComputer = new FTMCost(result.Matching);
            return new SimulationResultMutation
            {
                Mismatch           = result.NbMismatch,
                NoMatchUnjustified = result.NbNoMatchUnjustified,
                Label              = label,
                ComputationTime    = result.ComputationTime,
                NoMatch            = result.NbNoMatch,
                Total              = result.Total,
                NbMutationsMade    = mutationCouple.Mutant.NbMutations,
                MutantId           = mutationCouple.Mutant.Id.ToString(),
                OriginalId         = mutationCouple.Original.Id.ToString(),
                Success            = result.GoodMatches / (double) result.MaxGoodMatches,
                FTMCost            = ftmCostComputer.ComputeCost(),
                MutationsMade = mutationCouple.Mutant.MutationsMade
                    .ToLookup(m => m.MutationType, m => m)
                    .ToDictionary(m => m.Key, m => m.Count())
            };
        }
    }
}