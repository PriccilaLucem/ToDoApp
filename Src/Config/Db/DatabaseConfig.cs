using MongoDB.Driver;
using Microsoft.Extensions.Options;
using WebApplication.Src.Models;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Src.Config.Db
{
    public class DatabaseConfig : IDatabaseConfig
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<DatabaseConfig> _logger;
        private readonly MongoDBSettings _settings;

        public DatabaseConfig(
            IOptions<MongoDBSettings> settings,
            ILogger<DatabaseConfig> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            try
            {
                var clientSettings = MongoClientSettings.FromConnectionString(_settings.ConnectionString);
                clientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);

                var client = new MongoClient(clientSettings);
                _database = client.GetDatabase(_settings.DatabaseName);

                // Verifica a conexão
                _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();

                CreateIndexes();
                _logger.LogInformation("MongoDB connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize MongoDB connection");
                throw;
            }
        }

        private void CreateIndexes()
        {
            try
            {
                var usersCollection = GetCollection<UserModel>("Users");

                // Criando índice único para o campo Email
                var emailIndexKeys = Builders<UserModel>.IndexKeys.Ascending(u => u.Email);
                var emailIndexOptions = new CreateIndexOptions<UserModel>
                {
                    Unique = true,
                    PartialFilterExpression = Builders<UserModel>.Filter.Exists(u => u.Email, true)
                };

                var emailIndexModel = new CreateIndexModel<UserModel>(emailIndexKeys, emailIndexOptions);
                usersCollection.Indexes.CreateOne(emailIndexModel);

                _logger.LogInformation("MongoDB indexes created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create MongoDB indexes");
                throw;
            }
        }

        public IMongoCollection<T> GetCollection<T>(string? collectionName = null)
        {
            return _database.GetCollection<T>(collectionName ?? typeof(T).Name);
        }
    }

    public interface IDatabaseConfig
    {
        IMongoCollection<T> GetCollection<T>(string? collectionName = null);
    }

    public class MongoDBSettings
    {   

        [Required]
        public required string ConnectionString { get; set; }
        [Required]
        public string? DatabaseName { get; set; }

        public List<string> Collections { get; set; } = [];

    }
}
