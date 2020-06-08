using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Common;
using MongoDB.Bson;
using MoreLinq;

namespace tree_matching_csharp.Benchmark
{
    class Program
    {
        public static async Task RunAndSave(string label, ITreeMatcher matcher)
        {
            var repo    = await MongoRepository.InitConnection();
            var results = MutationBenchmark.Run(label, matcher);
            await foreach (var result in results)
            {
                if (result == null)
                    continue;
                repo.SaveResults(result);
                Console.WriteLine(result.ToJson());
            }
        }

        static async Task Main(string[] args)
        {
            var sftm = new SftmTreeMatcher(Settings.SFTMParameters);
            var rted = new RtedTreeMatcher(Settings.RTEDParameters);
            var tasks = new List<Task>();
            Enumerable.Range(0, Settings.ThreadsRTED)
                .ForEach(i => { tasks.Add(Task.Run(() => RunAndSave("RTED", rted))); });
            Enumerable.Range(0, Settings.ThreadsSFTM)
                .ForEach(i => { tasks.Add(Task.Run(() => RunAndSave("SFTM", sftm))); });
            
            foreach (var task in tasks)
                await task;
        }
    }
}