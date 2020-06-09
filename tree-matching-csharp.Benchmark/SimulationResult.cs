using System.Collections.Generic;
using MongoDB.Bson;

namespace tree_matching_csharp.Benchmark
{
    public class SimulationResult
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
}