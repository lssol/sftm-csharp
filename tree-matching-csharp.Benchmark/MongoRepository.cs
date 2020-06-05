using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace tree_matching_csharp.Benchmark
{
    public class DOMVersion
    {
        public ObjectId  Id            { get; set; }
        public string    Url           { get; set; }
        public string    Content       { get; set; }
        public int       Total         { get; set; }
        public ObjectId? Original      { get; set; }
        public BsonValue MutationsMade { get; set; }
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

    public class MongoRepository
    {
        private static   MongoRepository              _instance;
        private readonly IMongoCollection<DOMVersion> _mutationCollection;

        private MongoRepository(IMongoCollection<DOMVersion> mutationCollection)
        {
            _mutationCollection = mutationCollection;
        }

        public static MongoRepository InitConnection()
        {
            if (_instance != null)
                return _instance;
            var client   = new MongoClient(Settings.Mongo.ConnectionString);
            var database = client.GetDatabase(Settings.Mongo.BenchmarkDatabase);

            var pack = new ConventionPack {new CamelCaseElementNameConvention()};
            ConventionRegistry.Register("camel case", pack, t => true);

            var collection = database.GetCollection<DOMVersion>(Settings.Mongo.MutationCollection);
            _instance = new MongoRepository(collection);

            return _instance;
        }

        public IEnumerable<MutationCouple> GetCouples()
        {
            var builder = Builders<DOMVersion>.Filter;
            var filter = builder.Eq(doc => doc.Original, null) & builder.Lt(doc => doc.Total, 1000);
            var cursor = _mutationCollection.Find(filter).ToCursor();
            foreach (var original in cursor.ToEnumerable())
            {
                var filterBuilder = Builders<DOMVersion>.Filter;
                var f =
                    filterBuilder.Eq(d => d.Original, original.Id)
                    & filterBuilder.Gt(d => d.NbMutations, 0);
                var mutants = _mutationCollection.Find(f).ToList();
                foreach (var mutant in mutants)
                {
                    yield return new MutationCouple
                    {
                        Original = original,
                        Mutant   = mutant
                    };
                }
            }
        }
    }
}