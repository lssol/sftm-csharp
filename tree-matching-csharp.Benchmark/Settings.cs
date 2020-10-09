using System;

namespace tree_matching_csharp.Benchmark
{
    public static class Settings
    {
        public static class Mongo
        {
            public const  string ConnectionString               = "mongodb://wehave_prod%40service:AX3ohnia@datalakestar.amarislab.com:27018/?authMechanism=PLAIN&appname=tree-matching-csharp.benchmark&ssl=true";
            public const  string BenchmarkDatabase              = "locatorBenchmark";
            public const  string MutationCollection             = "DOMVersions";
            public const  string ResultCollection               = "VLDB_Mutation_SimulationResults_v3";
            public static string EdgeSimulationResultCollection = "VLDB_Mutation_EdgeSimulationResults";
        }

        public const string SFTMLabel = "SFTM";
        public const string RTEDLabel = "RTED";

        public static readonly SftmTreeMatcher.Parameters SFTMParameters = new SftmTreeMatcher.Parameters
        {
            LimitNeighbors = 50,
            MetropolisParameters = new Metropolis.Parameters
            {
                Gamma                   = 1f, // MUST be < 1
                Lambda                  = 2.5f,
                NbIterations            = 1,
                MetropolisNormalisation = true
            },
            NoMatchCost                    = 1.3,
            MaxPenalizationChildren        = 0.4,
            MaxPenalizationParentsChildren = 0.2,
            PropagationParameters = new SimilarityPropagation.Parameters
            {
                Envelop    = new[] {0.9, 0.1, 0.01},
                Parent     = 0.4,
                Sibling    = 0.0,
                SiblingInv = 0.0,
                ParentInv  = 0.9,
                Children   = 0.0
            },
            MaxTokenAppearance = n => (int) Math.Sqrt(n)
        };

        public const string UrlRTEDString  = "http://163.172.16.184:7040";
        public const string UrlRTEDDefault = "http://163.172.16.184:7041";
        
        public const string UrlXyDiff = "http://163.172.102.37:5000/match";

        public static readonly RtedTreeMatcher.Parameters RTEDParameters = new RtedTreeMatcher.Parameters
        {
            DeletionCost  = 1,
            InsertionCost = 1,
            RelabelCost   = 1,
        };

        public const  int MaxSizeWebsite     = 1500;
        public static int ThreadsRTED        = 2;
        public static int ThreadsXyDiff        = 1;
        public static int ThreadsRTEDDefault = 2;
        public static int ThreadsSFTM        = 2;
    }
}