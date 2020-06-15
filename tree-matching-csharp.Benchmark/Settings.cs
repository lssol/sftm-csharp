using System;

namespace tree_matching_csharp.Benchmark
{
    public static class Settings
    {
        public static class Mongo
        {
            public const string ConnectionString = "mongodb://wehave_prod%40service:AX3ohnia@datalakestar.amarislab.com:27018/?authMechanism=PLAIN&appname=tree-matching-csharp.benchmark&ssl=true";
            public const string BenchmarkDatabase = "locatorBenchmark";
            public const string MutationCollection = "DOMVersions";
            public const string ResultCollection = "VLDB_Mutation_SimulationResults";
        }

        public const string SFTMLabel = "SFTM";
        public const string RTEDLabel = "RTED";

        public static readonly SftmTreeMatcher.Parameters SFTMParameters = new SftmTreeMatcher.Parameters
        {
            LimitNeighbors = 2000,
            MetropolisParameters = new Metropolis.Parameters
            {
                Gamma        = 0.8f, // MUST be < 1
                Lambda       = 2.5f,
                NbIterations = 100,
            },
            NoMatchCost = 0.15,
            MaxPenalizationChildren = 0.5,
            MaxPenalizationParentsChildren = 0.2,
            PropagationParameters = new SimilarityPropagation.Parameters
            {
                Envelop    = new[] {7.9, 2.8, 0.1, 0.05, 0.01},
                // Envelop    = new[] {0.0},
                Parent     = 0.8,
                Sibling    = 0.1,
                SiblingInv = 0.05,
                ParentInv  = 0.9
            },
            // MaxTokenAppearance = n => (int) Math.Sqrt(n)
            MaxTokenAppearance = n => n
        };

        public const string UrlRTED = "http://163.172.16.184:7040";

        public static readonly RtedTreeMatcher.Parameters RTEDParameters = new RtedTreeMatcher.Parameters
        {
            DeletionCost = 1,
            InsertionCost = 1,
            RelabelCost = 1,
        };

        public const int MaxSizeWebsite = 1500;
        public static int ThreadsRTED = 4;
        public static int ThreadsSFTM = 2;
    }
}