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
            var results = MutationBenchmark.RunMutation(label, matcher);
            await foreach (var result in results)
            {
                if (result == null)
                    continue;
                // repo.SaveResults(result);
                Console.WriteLine(result.ToJson());
            }
        }

        public static async Task RunAndSaveBracket(string label, ITreeMatcher matcher)
        {
            var results = MutationBenchmark.RunBracket(label, matcher, "bolzano");
            var costs = new List<double>();
            var nomatch = new List<double>();
            var relabel = new List<double>();
            var ancestry = new List<double>();
            var sibling = new List<double>();
            const int maxLoops = 40;
            var i = 0;
            await foreach (var (source, target, result) in results)
            {
                // Console.WriteLine("###################################");
                // Console.WriteLine("New trees");
                // Console.WriteLine("###################################");
                // // Console.WriteLine("-------- SOURCE");
                // RtedTreeMatcher.ComputeChildren(source);
                // // Console.WriteLine("-------- TARGET");
                // RtedTreeMatcher.ComputeChildren(target);
                // PrintTree(source.First(s => s.Parent == null), "", true);
                // PrintTree(target.First(s => s.Parent == null), "", true);
                if (i == maxLoops)
                    break;
                costs.Add(result.FTMRelativeCost);
                nomatch.Add(result.NoMatch / (double) Math.Max(result.TotalSource, result.TotalTarget));
                relabel.Add(result.FTMCost.Relabel / (double) Math.Max(result.TotalSource, result.TotalTarget));
                ancestry.Add(result.FTMCost.Ancestry / (double) Math.Max(result.TotalSource, result.TotalTarget));
                sibling.Add(result.FTMCost.Sibling / (double) Math.Max(result.TotalSource, result.TotalTarget));
                i++;
            }
            Console.WriteLine($"Average ftm cost {label} = {costs.Average()}");
            Console.WriteLine($"Average nomatch {label} = {nomatch.Average()}");
            Console.WriteLine($"Average relabel {label} = {relabel.Average()}");
            Console.WriteLine($"Average ancestry {label} = {ancestry.Average()}");
            Console.WriteLine($"Average sibling {label} = {sibling.Average()}");
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
            
            // BOLZANO
            await RunAndSaveBracket("SFTM", sftm);
            // await RunAndSaveBracket("RTED", rted);

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