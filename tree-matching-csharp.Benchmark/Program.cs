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
        public static async Task RunAndSaveMutation(string label, ITreeMatcher matcher)
        {
            var repo    = await MongoRepository.InitConnection();
            var results = Benchmark.RunMutation(label, matcher);
            await foreach (var result in results)
            {
                if (result == null)
                    continue;
                repo.SaveResultsSimulation(result);
                Console.WriteLine(result.ToJson());
            }
        }

        public static async Task RunAndSaveBracket(string label, ITreeMatcher matcher)
        {
            var results = Benchmark.RunBracket(label, matcher, "bolzano");
            var resultList = new List<SimulationResultBracket>();
            
            const int maxLoops = 40;
            var i = 0;
            await foreach (var (source, target, result) in results)
            {
                if (i == maxLoops)
                    break;
                resultList.Add(result);     
                i++;
            }
            Console.WriteLine($"[{label}] Cost Average: {resultList.Average(r => r.CostAvg)}");
            Console.WriteLine($"[{label}] Precision: {resultList.Average(r => r.Precision)}");
            Console.WriteLine($"[{label}] Recall: {resultList.Average(r => r.Recall)}");
            Console.WriteLine($"[{label}] ComputationTime: {resultList.Average(r => r.ComputationTime)}");
        }

        public static async Task RunAndSaveEdgesSimulation(IEnumerable<(string, ITreeMatcher)> matchers)
        {
            var repo = await MongoRepository.InitConnection();
            var cursor = Benchmark.RunEdgeSimulation(matchers);
            await foreach (var edgeResult in cursor)
            {
                repo.SaveResultEdgesSimulation(edgeResult);
                Console.WriteLine(edgeResult.ToJson());
            }
        }
        

        public static void PrintTree(Node tree, String indent, bool last)
        {
            Console.WriteLine($"{indent}+-[{string.Join("", tree.Id.ToString().Take(3))}] {string.Join(" ", tree.Value)}");
            indent += last ? "   " : "|  ";

            for (int i = 0; i < tree.Children.Count; i++)
            {
                PrintTree(tree.Children[i], indent, i == tree.Children.Count - 1);
            }
        }
        
        static async Task Main(string[] args)
        {
            var sftm = new SftmTreeMatcher(Settings.SFTMParameters);
            var rted = new RtedTreeMatcher(Settings.RTEDParameters);

            // await RunAndSaveEdgesSimulation(new (string, ITreeMatcher)[] {("SFTM", sftm), ("RTED", rted)});
            
            await RunAndSaveBracket("SFTM", sftm);
            await RunAndSaveBracket("RTED", rted);

            // await RunAndSaveMutation("SFTM2", sftm);
            // MUTATION
            // var tasks = new List<Task>();


            // Enumerable.Range(0, Settings.ThreadsRTED)
            //     .ForEach(i => { tasks.Add(Task.Run(() => RunAndSaveMutation("RTED", rted))); });
            // Enumerable.Range(0, Settings.ThreadsSFTM)
            //     .ForEach(i => { tasks.Add(Task.Run(() => RunAndSaveMutation("SFTM", sftm))); });

            // foreach (var task in tasks)
            //     await task;
        }
    }
}