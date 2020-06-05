using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace tree_matching_csharp.Benchmark
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mutationBenchmark = new MutationBenchmark();
            var results =  mutationBenchmark.Run();
            Directory.CreateDirectory("results");
            await using var file = File.AppendText($"results/{DateTime.Now.ToFileTime()}");
            await foreach (var result in results)
            {
                var match = new
                {
                    Id              = result.MutationCouple.Mutant.Id.ToString(),
                    Label           = result.MatcherLabel,
                    Success         = 100 * ((double)result.GoodMatch / result.MaxGoodMatch),
                    ComputationTime = (int) result.MatchingDuration,
                    result.Total,
                    MutationPercentage = 100 * ((double) result.MutationsMade / result.Total)
                }.ToJson();
                
                file.WriteLine(match);
                Console.WriteLine(match);
                
                
            }
        }
    }
}