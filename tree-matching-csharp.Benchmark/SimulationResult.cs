using System.Collections.Generic;
using MongoDB.Bson;

namespace tree_matching_csharp.Benchmark
{
    public class SimulationResultMutation
    {
        public ObjectId                Id                 { get; set; }
        public string                  OriginalId         { get; set; }
        public string                  MutantId           { get; set; }
        public string                  Label              { get; set; }
        public double                  Success            { get; set; }
        public long                    ComputationTime    { get; set; }
        public int                     Total              { get; set; }
        public int                     NoMatch            { get; set; }
        public int                     NoMatchUnjustified { get; set; }
        public int                     Mismatch           { get; set; }
        public int                     NbMutationsMade    { get; set; }
        public Dictionary<string, int> MutationsMade      { get; set; }
        public FTMCost.Cost            FTMCost            { get; set; }
    }

    public class SimulationResultBracket
    {
        public string       Id              { get; set; }
        public string       Dataset         { get; set; }
        public long         ComputationTime { get; set; }
        public string       MatcherLabel    { get; set; }
        public int          NoMatch         { get; set; }
        public FTMCost.Cost FTMCost         { get; set; }
        public int TotalSource { get; set; }
        public int TotalTarget { get; set; }
        public double FTMRelativeCost { get; set; }
    }
}