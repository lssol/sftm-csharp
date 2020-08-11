using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
        public FtmCost.Cost            FTMCost            { get; set; }
    }

    public class SimulationResultBracket
    {
        public string       Id              { get; set; }
        public string       Dataset         { get; set; }
        public long         ComputationTime { get; set; }
        public string       MatcherLabel    { get; set; }
        public int          NoMatch         { get; set; }
        public FtmCost.Cost FTMCost         { get; set; }
        public int TotalSource { get; set; }
        public int TotalTarget { get; set; }
        public double FTMRelativeCost { get; set; }
        public IEnumerable<Edge> Matching { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double CostAvg { get; set; }
    }
    
    [BsonIgnoreExtraElements]
    public class EdgeSimulationResult
    {
        public string MutantId { get; set; }
        public string OriginalId { get; set; }
        
        public string MatcherLabel { get; set; }
        
        public string SourceSignature { get; set; }
        public string TargetSignature { get; set; }
        
        public bool IsCorrect { get; set; }
        
        public int NbSiblingsSource  { get; set; }
        public int NbSiblingsTarget  { get; set; }
        public int NbChildrenSource  { get; set; }
        public int NbChildrenTarget  { get; set; }
        
        public int TotalNodesSource { get; set; }
        public int TotalNodesTarget { get; set; }
        
        public double RelabelCost  { get; set; }
        public double SiblingCost  { get; set; }
        public double AncestryCost  { get; set; }
        
    }
}