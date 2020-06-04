using System;
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
            await foreach (var result in results)
            {
                Console.WriteLine(new
                {
                    Success         = 100 * ((double)result.GoodMatch / result.MaxGoodMatch),
                    ComputationTime = (int) result.MatchingDuration,
                    Total = result.Total,
                    MutationPercentage = 100 * ((double) result.MutationsMade / result.Total)
                }.ToJson());
            }
        }
    }
}