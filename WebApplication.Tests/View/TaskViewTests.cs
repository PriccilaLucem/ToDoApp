using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using WebApplication.Src.Interface;
using WebApplication.Src.Models.TaskModel;
using Xunit;
using WebApplication.Src.View;
using WebApplication.Src.Config.Db;
using Microsoft.Extensions.Configuration;

namespace WebApplication.WebApplication.Tests.View
{
    public class TaskViewsTests
    {
        private readonly Mock<IMongoCollection<TaskModel>> _mockTaskCollection;
        private readonly Mock<ILogger<TaskViews>> _mockLogger;
        private readonly Mock<IDatabaseConfig> _mockDatabaseConfig;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly TaskViews _taskViews;

        public TaskViewsTests()
        {
            _mockTaskCollection = new Mock<IMongoCollection<TaskModel>>();
            _mockLogger = new Mock<ILogger<TaskViews>>();
            _mockDatabaseConfig = new Mock<IDatabaseConfig>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["MongoDBSettings:Collections:Tasks"]).Returns("Tasks");
            _mockDatabaseConfig.Setup(d => d.GetCollection<TaskModel>(It.IsAny<string>())).Returns(_mockTaskCollection.Object);

            _taskViews = new TaskViews(_mockDatabaseConfig.Object, _mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ListTaskView_ReturnsListOfTasks()
        {
            var tasks = new List<TaskModel> { new TaskModel { Id = "1", Title = "Test Task" } };
            var mockCursor = new Mock<IAsyncCursor<TaskModel>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(default)).ReturnsAsync(true).ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(tasks);
            _mockTaskCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<TaskModel>>(), It.IsAny<FindOptions<TaskModel>>(), default))
                .ReturnsAsync(mockCursor.Object);

            var result = await _taskViews.ListTaskView();
            Assert.Single(result);
            Assert.Equal("Test Task", result[0].Title);
        }

        [Fact]
        public async Task DeleteTaskView_ReturnsTrue_WhenTaskDeleted()
        {
            var deleteResult = new DeleteResult.Acknowledged(1);
            _mockTaskCollection.Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<TaskModel>>(), default))
                .ReturnsAsync(deleteResult);

            var result = await _taskViews.DeleteTaskView("60d5f5a56e4d5b001cfd4a1a");
            Assert.True(result);
        }

        [Fact]
        public async Task CreateTaskView_ReturnsTaskId()
        {
            var task = new TaskModel { Id = "123", Title = "New Task" };
            _mockTaskCollection.Setup(c => c.InsertOneAsync(task, null, default));

            var result = await _taskViews.CreateTaskView(task);
            Assert.Equal("123", result);
        }

        [Fact]
        public async Task UpdateTaskView_ReturnsUpdatedTask()
        {
            var task = new TaskModel { Id = "123", Title = "Updated Task" };
            _mockTaskCollection.Setup(c => c.FindOneAndReplaceAsync(
                It.IsAny<FilterDefinition<TaskModel>>(), 
                task,
                It.IsAny<FindOneAndReplaceOptions<TaskModel>>(),
                default
            )).ReturnsAsync(task);

            var result = await _taskViews.UpdateTaskView(task);
            Assert.Equal("Updated Task", result.Title);
        }

        [Fact]
        public async Task GetOneTaskView_ReturnsTask_WhenFound()
        {
            var task = new TaskModel { Id = "123", Title = "Task Found" };
            var mockCursor = new Mock<IAsyncCursor<TaskModel>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(default)).ReturnsAsync(true).ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<TaskModel> { task });
            _mockTaskCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<TaskModel>>(), It.IsAny<FindOptions<TaskModel>>(), default))
                .ReturnsAsync(mockCursor.Object);

            var result = await _taskViews.GetOneTaskView("123");
            Assert.NotNull(result);
            Assert.Equal("Task Found", result.Title);
        }
    }
}
