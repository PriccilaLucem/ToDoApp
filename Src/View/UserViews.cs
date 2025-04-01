using MongoDB.Driver;
using MongoDB.Bson;
using WebApplication.Src.Config.Db;
using WebApplication.Src.Models;
using WebApplication.Src.Interface;

namespace WebApplication.View
{
    public class UserViews : IUserInterface
    {
        private readonly IMongoCollection<UserModel> _usersCollection;
        private readonly ILogger<UserViews> _logger;

        public UserViews(IDatabaseConfig databaseConfig, IConfiguration config, ILogger<UserViews> logger) 
        {
            var collectionName = config["MongoDBSettings:Collections:Users"] ?? "Users";
            
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException(nameof(config), "Collection name is not configured in appsettings.json");
            }

            _usersCollection = databaseConfig.GetCollection<UserModel>(collectionName);
            _logger = logger;
        }    

        public Task<UserModel> GetUserByEmail(string email)
        {
            return _usersCollection.Find(user => user.Email == email).FirstAsync();
        }
        public async Task<List<UserModel>> GetUsers()
        {
            try
            {
                return await _usersCollection.Find(_ => true).ToListAsync();
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error getting users");
                throw;
            }
        }

        public async Task<UserModel?> GetUserByIdAsync(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out _))
                {
                    return null;
                }

                return await _usersCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, $"Error getting user with id {id}");
                throw;
            }
        }

        public async Task<string> CreateUsers(UserModel user)
        {
            try
            {
                await _usersCollection.InsertOneAsync(user);
                return user.Id;
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<bool> DeleteUsers(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    return false;
                }

                var result = await _usersCollection.DeleteOneAsync(user => user.Id == objectId.ToString());
                return result.DeletedCount > 0;
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, $"Error deleting user with id {id}");
                throw;
            }
        }

        public async Task<UserModel> UpdateUsers(UserModel user) 
        {
            try
            {
                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    throw new ArgumentException("Invalid user data");
                }

                var filter = Builders<UserModel>.Filter.Eq(u => u.Id, user.Id);
                var options = new FindOneAndReplaceOptions<UserModel>
                {
                    ReturnDocument = ReturnDocument.After
                };

                return await _usersCollection.FindOneAndReplaceAsync(filter, user, options);
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, $"Error updating user with id {user?.Id}");
                throw;
            }
        }
    }
}