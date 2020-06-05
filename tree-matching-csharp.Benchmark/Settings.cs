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
        }

        public const string SFTMLabel = "SFTM";
        public const string RTEDLabel = "RTED";

        public static readonly SftmTreeMatcher.Parameters SFTMParameters = new SftmTreeMatcher.Parameters
        {
            LimitNeighbors = 20,
            MetropolisParameters = new Metropolis.Parameters
            {
                Gamma        = 1f,
                Lambda       = 0.7f,
                NbIterations = 20,
            },
            NoMatchCost = 0.2,
            PropagationParameters = new SimilarityPropagation.Parameters()
            {
                Envelop    = new[] {0.7, 0.08},
                Parent     = 0.2,
                Sibling    = 0.1,
                SiblingInv = 0.1,
                ParentInv  = 0.6
            },
            MaxTokenAppearance = n => (int) Math.Sqrt(n)
        };

        public const string UrlRTED = "http://163.172.16.184:7040";

        public static readonly RTED.Parameters RTEDParameters = new RTED.Parameters
        {
            DeletionCost = 1,
            InsertionCost = 1,
            RelabelCost = 1,
        };
    }
}