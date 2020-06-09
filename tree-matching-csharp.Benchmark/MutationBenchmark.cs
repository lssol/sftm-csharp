using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;

namespace tree_matching_csharp.Benchmark
{
    public static class MutationBenchmark
    {
        private static SimulationResult ToSimulationResult(WebsiteMatcher.Result result, MutationCouple mutationCouple, string label)
        {
            var ftmCostComputer = new FTMCost(result.Matching);
            return new SimulationResult
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

        public static async IAsyncEnumerable<SimulationResult> Run(string label, ITreeMatcher matcher)
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
    }
}