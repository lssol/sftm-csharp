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
            public int GoodMatch { get; set; }
            public int MaxGoodMatch { get; set; }
        }

        public async IAsyncEnumerable<Result> RunSFTM()
        {
            var mongoRepo = MongoRepository.InitConnection();
            foreach (var (original, mutant) in mongoRepo.GetCouples())
            {
                var treeMatcher = new TreeMatcher(Settings.SFTMParameters);

                var result = await treeMatcher.MatchWebsites(original.Content, mutant.Content);
                yield return new Result
                {
                    Matches            = result.SignatureMatching,
                    Mismatch           = result.NbMismatch,
                    NoMatchUnjustified = result.NbNoMatchUnjustified,
                    MatcherLabel       = Settings.SFTMLabel,
                    MatchingDuration   = result.ComputationTime,
                    MutationCouple     = new MutationCouple {Mutant = mutant, Original = original},
                    NoMatch            = result.NbNoMatch,
                    Total              = result.Total,
                    MutationsMade      = mutant.NbMutations,
                    MaxGoodMatch = result.MaxGoodMatches,
                    GoodMatch = result.GoodMatches
                };
            }
        }
    }
}