using MongoDB.Bson;
using MongoDB.Driver;
using WebApplication.Src.Config.Db;
using WebApplication.Src.Interface;
using WebApplication.Src.Models.TaskModel;

namespace WebApplication.View
{
    public class TaskViews: TaskInterface
    {
        private readonly IMongoCollection<TaskModel> _taskCollection;
        private readonly ILogger<TaskViews> _logger;
        public TaskViews(IDatabaseConfig databaseConfig, IConfiguration configuration, ILogger<TaskViews> logger)
        {
            var collectionName = configuration["MongoDBSettings:Collections:Tasks"] ?? "Tasks";
            if(string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException(nameof(configuration), "Collection name is not configured in appsettings.json");
            }
            _taskCollection = databaseConfig.GetCollection<TaskModel>(collectionName); 
            _logger = logger;
        }

        public async  Task<List<TaskModel>> ListTaskView()
        {
            try
            {
                return await _taskCollection.Find(_=> true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tasks");
                throw;
            }
        }
        public async Task<bool> DeleteTaskView(string id)
        {
            try
            {
                if(!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    return false;
                }
                var result = await _taskCollection.DeleteOneAsync(task=> task.Id == objectId.ToString());
                return result.DeletedCount > 0;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task with id {id}");
                throw;
            }
        }
        public async Task<string> CreateTaskView(TaskModel task)
        {
            try
            {
                await _taskCollection.InsertOneAsync(task);
                return task.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                throw;
            }
        }
        public async Task<TaskModel> UpdateTaskView(TaskModel task)
        {
            try
            {
                var filter = Builders<TaskModel>.Filter.Eq(t => t.Id, task.Id);
                var options = new FindOneAndReplaceOptions<TaskModel>
                {
                    ReturnDocument = ReturnDocument.After
                };
                
                task.UpdatedAt = DateTime.Now;
                return await _taskCollection.FindOneAndReplaceAsync(filter, task, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updateing task with id {task?.Id}");
                throw;
                
            }
        }
        public async Task<TaskModel> GetOneTaskView(string taskId)
        {
            try
            {
                return await _taskCollection.Find(item => item.Id == taskId).FirstAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at getting one task view");
                throw;
            }
        }
    }

}