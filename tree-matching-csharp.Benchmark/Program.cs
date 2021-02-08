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
        public static async Task RunAndSaveMutation(string label, IWebsiteMatcher matcher, int? limit = null)
        {
            var repo    = await MongoRepository.InitConnection();
            var results = Benchmark.RunMutation(label, matcher, limit);
            await foreach (var result in results)
            {
                if (result == null)
                    continue;
                repo.SaveResultsSimulation(result);
                // Console.WriteLine(result.ToJson());
                Console.WriteLine($"*********** {label} ************");
                Console.WriteLine($"Good Match: {result.GoodMatches}");
                Console.WriteLine($"Success: {result.Success}");
                Console.WriteLine($"Time: {result.ComputationTime}");
            }
        }

        public static async Task RunAndSaveBracket(string label, ITreeMatcher matcher)
        {
            var results    = Benchmark.RunBracket(label, matcher, "bolzano");
            var resultList = new List<SimulationResultBracket>();

            const int maxLoops = 40;
            var       i        = 0;
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
            var repo   = await MongoRepository.InitConnection();
            var cursor = Benchmark.RunEdgeSimulation(matchers);
            await foreach (var edgeResult in cursor)
            {
                // repo.SaveResultEdgesSimulation(edgeResult);
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
            var sftm        = new SftmTreeMatcher(Settings.SFTMParameters());
            var alphas = new List<double>{0.15, 0.20, 0.25, 0.30, 0.35, 0.40, 0.45,
                0.50, 0.55, 0.60, 0.65, 0.70, 0.75, 0.80, 0.85};
            var sftms = alphas.Select(alpha =>
            {
                var p = Settings.SFTMParameters();
                p.MaxTokenAppearance = i => (int) Math.Round(Math.Pow(i, alpha));
                return (new SftmTreeMatcher(p), $"sftm_alpha_{alpha}");
            });
            var rtedDefault = new RtedTreeMatcher(new RtedTreeMatcher.Parameters
            {
                DeletionCost = Settings.RTEDParameters.DeletionCost,
                InsertionCost = Settings.RTEDParameters.InsertionCost,
                RelabelCost = Settings.RTEDParameters.RelabelCost,
                LabelCostFunction = RtedTreeMatcher.LabelCostFunction.Default
            });
            var rtedString = new RtedTreeMatcher(new RtedTreeMatcher.Parameters
            {
                DeletionCost = Settings.RTEDParameters.DeletionCost,
                InsertionCost = Settings.RTEDParameters.InsertionCost,
                RelabelCost = Settings.RTEDParameters.RelabelCost,
                LabelCostFunction = RtedTreeMatcher.LabelCostFunction.String
            });

            // await RunAndSaveEdgesSimulation(new (string, ITreeMatcher)[] {("RTED-default", rtedDefault)});

            // await RunAndSaveBracket("SFTM", sftm);
            // await RunAndSaveBracket("RTED-default", rtedDefault);

            // await RunAndSaveMutation("sftm_with_content_and_prefix", new WebsiteMatcher(sftm));
            // await RunAndSaveMutation("RTED-default", new WebsiteMatcher(rtedString), 5);
            // Console.WriteLine("*************");
            // await RunAndSaveMutation("xydiff", new XyDiffMatcher(), 5);
            
            // MUTATION
            var tasks = new List<Task>();
            
            // Enumerable.Range(0, Settings.ThreadsRTED)
            //     .ForEach(i => { tasks.Add(Task.Run(() => RunAndSaveMutation("RTED", new WebsiteMatcher(rtedString)))); });
            // Enumerable.Range(0, Settings.ThreadsRTEDDefault)
            //     .ForEach(i => { tasks.Add(Task.Run(() => RunAndSaveMutation("RTED-default", new WebsiteMatcher(rtedDefault)))); });
            Enumerable.Range(0, Settings.ThreadsSFTM)
                .ForEach(i => { tasks.Add(Task.Run(() => RunAndSaveMutation("sftm_prefix_content_single_thread", new WebsiteMatcher(sftm)))); });
            // Enumerable.Range(0, Settings.ThreadsXyDiff)
            //     .ForEach(i => { tasks.Add(Task.Run(() => RunAndSaveMutation("xydiff3", new XyDiffMatcher()))); });

            // sftms.ForEach(tuple =>
            // {
            //     var (s, label) = tuple;
            //     tasks.Add(Task.Run(() => RunAndSaveMutation(label, new WebsiteMatcher(s))));
            // });
            //
            foreach (var task in tasks)
                await task;
        }
    }
}