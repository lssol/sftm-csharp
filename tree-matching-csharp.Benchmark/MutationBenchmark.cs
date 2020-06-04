using System;
using System.Collections;
using System.Collections.Generic;
using MoreLinq.Extensions;

namespace tree_matching_csharp.Benchmark
{
    public class MutationBenchmark
    {
        public class Result
        {
            public string                        MatcherLabel       { get; set; }
            public int                           NoMatch            { get; set; }
            public int                           NoMatchUnjustified { get; set; }
            public int                           Mismatch           { get; set; }
            public long                          MatchingDuration   { get; set; }
            public IEnumerable<(string, string)> Matches            { get; set; }
            public MutationCouple                MutationCouple     { get; set; }
            public int                           Total              { get; set; }
            public int                           MutationsMade      { get; set; }
            public int                           GoodMatch          { get; set; }
            public int                           MaxGoodMatch       { get; set; }
        }

        private Result ToOutput(WebsiteMatcher.Result result, MutationCouple mutationCouple)
        {
            return new Result
            {
                Matches            = result.SignatureMatching,
                Mismatch           = result.NbMismatch,
                NoMatchUnjustified = result.NbNoMatchUnjustified,
                MatcherLabel       = Settings.SFTMLabel,
                MatchingDuration   = result.ComputationTime,
                MutationCouple     = mutationCouple,
                NoMatch            = result.NbNoMatch,
                Total              = result.Total,
                MutationsMade      = mutationCouple.Mutant.NbMutations,
                MaxGoodMatch       = result.MaxGoodMatches,
                GoodMatch          = result.GoodMatches
            };
        }

        public async IAsyncEnumerable<Result> Run()
        {
            var sftm               = new SftmTreeMatcher(Settings.SFTMParameters);
            var rted               = new RTED(Settings.RTEDParameters);
            var sftmWebsiteMatcher = new WebsiteMatcher(sftm);
            var rtedWebsiteMatcher = new WebsiteMatcher(rted);
            var mongoRepo          = MongoRepository.InitConnection();
            foreach (var (original, mutant) in mongoRepo.GetCouples())
            {
                // var resultRTED = await rtedWebsiteMatcher.MatchWebsites(original.Content, mutant.Content);
                var resultSFTM = await sftmWebsiteMatcher.MatchWebsites(original.Content, mutant.Content);
                var mutationCouple = new MutationCouple{Mutant = mutant, Original = original};
                
                yield return ToOutput(resultSFTM, mutationCouple);
                // yield return ToOutput(resultRTED, mutationCouple);
            }
        }
    }
}