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

        public static readonly TreeMatcher.Parameters SFTMParameters = new TreeMatcher.Parameters
        {
            LimitNeighbors = 10,
            MetropolisParameters = new Metropolis.Parameters
            {
                Gamma        = 1f,
                Lambda       = 0.7f,
                NbIterations = 20,
            },
            NoMatchCost = 0.2,
            PropagationParameters = new SimilarityPropagation.Parameters()
            {
                Envelop    = new[] {0.7, 0.05},
                Parent     = 0.0,
                Sibling    = 0.0,
                SiblingInv = 0.0,
                ParentInv  = 0.6
            },
            MaxTokenAppearance = n => (int) Math.Sqrt(n)
        };
    }
}