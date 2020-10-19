using System;
using System.Collections.Generic;
using System.Linq;

namespace tree_matching_csharp.Benchmark
{
    public static class Benchmark
    {
        public static async IAsyncEnumerable<SimulationResultMutation?> RunMutation(string label,
            IWebsiteMatcher matcher, int? limit = null)
        {
            var mongoRepo = await MongoRepository.InitConnection();
            foreach (var (original, mutant) in mongoRepo.GetCouples(limit))
            {
                if (await mongoRepo.MeasureAlreadyExists(label, mutant.Id.ToString()))
                {
                    Console.WriteLine("Skipped couple");
                    continue;
                }

                WebsiteMatcher.Result? results;
                try
                {
                    results = await matcher.MatchWebsites(original.Content, mutant.Content);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Continuing anyway...");
                    continue;
                }

                var mutationCouple = new MutationCouple {Mutant = mutant, Original = original};

                yield return results == null ? null : ToSimulationResult(results, mutationCouple, label);
            }
        }

        public static async IAsyncEnumerable<EdgeSimulationResult> RunEdgeSimulation(
            IEnumerable<(string, ITreeMatcher)> matchers)
        {
            var mongoRepo = await MongoRepository.InitConnection();
            foreach (var (original, mutant) in mongoRepo.GetCouples())
            foreach (var (name, matcher) in matchers)
            {
                if (await mongoRepo.EdgeAlreadyExists(name, mutant.Id.ToString()))
                {
                    Console.WriteLine("Skipped couple");
                    continue;
                }

                var websiteMatcher = new WebsiteMatcher(matcher);
                WebsiteMatcher.Result? results;
                try
                {
                    results = await websiteMatcher.MatchWebsites(original.Content, mutant.Content);
                    if (results == null)
                        throw new Exception("No results from website matching");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Continuing anyway...");
                    continue;
                }

                var edges = results.Matching.ToList();
                new FtmCost(edges).ComputeCost();
                foreach (var edge in edges)
                {
                    if (edge.Source == null || edge.Target == null)
                        continue;

                    yield return new EdgeSimulationResult
                    {
                        MutantId = mutant.Id.ToString(),
                        OriginalId = original.Id.ToString(),
                        SourceSignature = edge.Source.Signature,
                        TargetSignature = edge.Target.Signature,
                        TotalNodesSource = original.Total,
                        TotalNodesTarget = mutant.Total,
                        IsCorrect = edge.Source.Signature == edge.Target.Signature,
                        NbChildrenSource = edge.Source.Children.Count,
                        NbChildrenTarget = edge.Target.Children.Count,
                        NbSiblingsSource = edge.Source.Parent?.Children.Count ?? 0,
                        NbSiblingsTarget = edge.Target.Parent?.Children.Count ?? 0,
                        AncestryCost = edge.FtmCost.Ancestry,
                        RelabelCost = edge.FtmCost.Relabel,
                        SiblingCost = edge.FtmCost.Sibling,
                        MatcherLabel = name
                    };
                }
            }
        }

        public static async
            IAsyncEnumerable<(IEnumerable<Node> source, IEnumerable<Node> target, SimulationResultBracket)> RunBracket(
                string label, ITreeMatcher matcher, string dataset)
        {
            foreach (var (key, source, target) in BolzanoImporter.GetBolzanoTrees())
            {
                TreeMatcherResponse resultMatching;
                try
                {
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
                var edges = resultMatching.Edges.ToList();
                var ftmCost = new FtmCost(edges).ComputeCost();
                var ftmRelativeCost =
                    (ftmCost.Ancestry + ftmCost.Relabel + ftmCost.Sibling + ftmCost.NoMatch) / maxTotal;
                var ftmPrecisionAvg = edges.Average(e => e.FtmCost.Ancestry + e.FtmCost.Relabel + e.FtmCost.Sibling);

                var edgesMatched = edges.Count(e => e.FtmCost.NoMatch == 0);
                var precision = edgesMatched == 0 ? 1 : (edges.Count(e => e.FtmCost.IsCorrect) / edgesMatched);
                var recall = edges.Count(e => e.FtmCost.IsCorrect) / (double) Math.Min(source.Count(), target.Count());
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
                    FTMRelativeCost = ftmRelativeCost,
                    Matching = resultMatching.Edges,
                    Precision = precision,
                    Recall = recall,
                    CostAvg = ftmPrecisionAvg
                });
            }
        }

        private static SimulationResultMutation ToSimulationResult(WebsiteMatcher.Result result,
            MutationCouple mutationCouple, string label)
        {
            // var ftmCostComputer = new FtmCost(result.Matching.ToList());
            var (original, mutant) = mutationCouple;
            return new SimulationResultMutation
            {
                Mismatch = result.NbMismatch,
                NoMatchUnjustified = result.NbNoMatchUnjustified,
                Label = label,
                ComputationTime = result.ComputationTime,
                NoMatch = result.NbNoMatch,
                MaxGoodMatch = result.MaxGoodMatches,
                Total = result.Total,
                NbMutationsMade = mutant.NbMutations,
                MutantId = mutant.Id.ToString(),
                OriginalId = original.Id.ToString(),
                Success = result.GoodMatches / (double) result.MaxGoodMatches,
                GoodMatches = result.GoodMatches,
                // FTMCost            = ftmCostComputer.ComputeCost(),
                MutationsMade = mutant.MutationsMade
                    .ToLookup(m => m.MutationType, m => m)
                    .ToDictionary(m => m.Key, m => m.Count())
            };
        }
    }
}