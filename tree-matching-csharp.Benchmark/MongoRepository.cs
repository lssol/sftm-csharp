using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace tree_matching_csharp.Benchmark
{
    public class MongoRepository
    {
        private static   MongoRepository              _instance;
        private readonly IMongoCollection<DOMVersion> _mutationCollection;
        private readonly IMongoCollection<SimulationResultMutation> _simulationResultsCollection;
        private readonly IMongoCollection<EdgeSimulationResult> _edgeSimulationResultCollection;

        private MongoRepository(IMongoCollection<DOMVersion> mutationCollection, IMongoCollection<SimulationResultMutation> simulationResultsCollection, IMongoCollection<EdgeSimulationResult> edgeSimulationResultCollection)
        {
            _mutationCollection = mutationCollection;
            _simulationResultsCollection = simulationResultsCollection;
            _edgeSimulationResultCollection = edgeSimulationResultCollection;
        }

        public static async Task<MongoRepository> InitConnection()
        {
            if (_instance != null)
                return _instance;
            var client   = new MongoClient(Settings.Mongo.ConnectionString);
            var database = client.GetDatabase(Settings.Mongo.BenchmarkDatabase);

            var pack = new ConventionPack {new CamelCaseElementNameConvention()};
            ConventionRegistry.Register("camel case", pack, t => true);

            var mutationCollection = database.GetCollection<DOMVersion>(Settings.Mongo.MutationCollection);
            var simulationResultCollection = database.GetCollection<SimulationResultMutation>(Settings.Mongo.ResultCollection);
            var edgeSimulationResultCollection = database.GetCollection<EdgeSimulationResult>(Settings.Mongo.EdgeSimulationResultCollection);
            await simulationResultCollection.Indexes?.CreateOneAsync(Builders<SimulationResultMutation>.IndexKeys.Ascending(s => s.Label));
            await simulationResultCollection.Indexes?.CreateOneAsync(Builders<SimulationResultMutation>.IndexKeys.Ascending(s => s.OriginalId));
            await simulationResultCollection.Indexes?.CreateOneAsync(Builders<SimulationResultMutation>.IndexKeys.Ascending(s => s.MutantId));
            _instance = new MongoRepository(mutationCollection, simulationResultCollection, edgeSimulationResultCollection);

            return _instance;
        }

        public void SaveResultsSimulation(SimulationResultMutation resultMutation)
        {
            _simulationResultsCollection.InsertOne(resultMutation);
        }

        public void SaveResultEdgesSimulation(EdgeSimulationResult results)
        {
            _edgeSimulationResultCollection.InsertOne(results);
        }

        public async Task<bool> MeasureAlreadyExists(string label, string mutantId)
        {
            return await _simulationResultsCollection.AsQueryable()
                .AnyAsync(s => s.Label == label && s.MutantId == mutantId);
        }
        
        public async Task<bool> EdgeAlreadyExists(string label, string mutantId)
        {
            return await _edgeSimulationResultCollection.AsQueryable()
                .AnyAsync(s => s.MatcherLabel == label && s.MutantId == mutantId);
        }

        public IEnumerable<MutationCouple> GetCouples(int? limit = null)
        {
            var builder = Builders<DOMVersion>.Filter;
            var filter  = builder.Eq(doc => doc.Original, null) & builder.Lt(doc => doc.Total, Settings.MaxSizeWebsite);
            var cursor  = _mutationCollection.Find(filter, new FindOptions{NoCursorTimeout = true}).Limit(limit).ToCursor();
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