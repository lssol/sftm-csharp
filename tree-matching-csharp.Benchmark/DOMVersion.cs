using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace tree_matching_csharp.Benchmark
{
    public class MutationMade
    {
        [BsonElement("MutationType")]
        public string MutationType     { get; set; }
        [BsonElement("MutationCategory")]
        public string MutationCategory { get; set; }
        [BsonElement("ElementConcerned")]
        public BsonValue ElementConcerned { get; set; }
        [BsonElement("AttributeConcerned")]
        public BsonValue AttributeConcerned { get; set; }
    }
    
    [BsonIgnoreExtraElements]
    public class DOMVersion
    {
        public ObjectId  Id            { get; set; }
        public string    Url           { get; set; }
        public string    Content       { get; set; }
        public int       Total         { get; set; }
        public ObjectId? Original      { get; set; }
        public IEnumerable<MutationMade> MutationsMade { get; set; }
        public int       NbMutations   { get; set; }
    }
    
    public class MutationCouple
    {
        public DOMVersion Original { get; set; }
        public DOMVersion Mutant   { get; set; }

        public void Deconstruct(out DOMVersion original, out DOMVersion mutant)
        {
            original = Original;
            mutant   = Mutant;
        }
    }
}