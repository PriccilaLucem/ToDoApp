using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WebApplication.Src.Dto.task;
using WebApplication.Src.Interface;
using WebApplication.Src.Models.TaskModel;
using Xunit;
using Xunit.Abstractions;
using WebApplication.Tests.Integration;
using WebApplication.WebApplication.Tests.Util;

namespace WebApplication.Tests.IntegrationTest
{
    public class TaskControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly Mock<ITaskViews> _taskViewsMock = new();
        private readonly TestLogger _logger;

        public TaskControllerIntegrationTest(ITestOutputHelper output)
        {
            _factory = new CustomWebApplicationFactory();
            _logger = new TestLogger(output);
            
            _factory.AddServiceConfiguration(services => 
            {
                services.AddScoped<ITaskViews>(_ => _taskViewsMock.Object);
            });
            
            _client = _factory.GetAuthenticatedClient(userId: "67eac582b221fb76523d8689");
            
            _logger.LogInfo("Test initialized");
        }

        [Fact]
        public async Task Post_ReturnsCreated_WhenTaskIsValid()
        {
            try
            {
                _logger.LogInfo("Starting Post_ReturnsCreated_WhenTaskIsValid test");
                
                // Arrange
                var taskDto = new TaskDTO
                {
                    Title = "Task Title Example",
                    Description = "This is an example description of the task.",
                    Category = "Personal Care",
                    DueDate = DateTime.Parse("2025-05-01T14:00:00"),
                    Priority = 2,
                    IsCompleted = false,
                    DurationMinutes = 120,
                    Tags = new List<string> { "Tag1", "Tag2", "Tag3" },
                    Recurrence = new RecurrencePatternDTO
                    {
                        Type = RecurrencePatternType.Daily,
                        Interval = 1,
                        DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
                        EndDate = DateTime.Parse("2025-06-01T00:00:00"),
                        MaxOccurrences = 10,
                        ExceptionDates = new List<DateTime>
                        {
                            DateTime.Parse("2025-05-05T00:00:00"),
                            DateTime.Parse("2025-05-12T00:00:00")
                        }
                    },
                    UserId = "67eac582b221fb76523d8689"
                };
                
                var expectedId = "123";
                
                _taskViewsMock.Setup(v => v.CreateTaskView(It.IsAny<TaskModel>()))
                            .ReturnsAsync(expectedId);

                // Act
                var response = await _client.PostAsJsonAsync("/api/v1/task", taskDto);

                // Assert
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                var content = await response.Content.ReadFromJsonAsync<object>();
                Assert.NotNull(content);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task GetOneTask_ReturnsTask_WhenExists()
        {
            try
            {
                _logger.LogInfo("Starting GetOneTask_ReturnsTask_WhenExists test");
                
                // Arrange
                var taskId = "123";
                var expectedTask = new TaskModel { Id = taskId, Title = "Test Task" };
                
                _taskViewsMock.Setup(v => v.GetOneTaskView(taskId))
                             .ReturnsAsync(expectedTask);

                // Act
                var response = await _client.GetAsync($"/api/v1/task/{taskId}");

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var task = await response.Content.ReadFromJsonAsync<TaskModel>();
                Assert.Equal(taskId, task?.Id);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task GetAllTasks_ReturnsTaskList()
        {
            try
            {
                _logger.LogInfo("Starting GetAllTasks_ReturnsTaskList test");
                
                // Arrange
                var expectedTasks = new List<TaskModel> 
                { 
                    new TaskModel { Id = "1" }, 
                    new TaskModel { Id = "2" } 
                };
                
                _taskViewsMock.Setup(v => v.ListTaskView())
                             .ReturnsAsync(expectedTasks);

                // Act
                var response = await _client.GetAsync("/api/v1/task");

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var tasks = await response.Content.ReadFromJsonAsync<TaskModel[]>();
                Assert.Equal(2, tasks?.Length);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task DeleteTask_ReturnsNoContent_WhenTaskExists()
        {
            try
            {
                _logger.LogInfo("Starting DeleteTask_ReturnsNotFound_WhenNotExists test");
                
                var taskId = "123";
                
                _taskViewsMock.Setup(v => v.DeleteTaskView(taskId))
                             .ReturnsAsync(true);

                var response = await _client.DeleteAsync($"/api/v1/task/{taskId}");

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task UpdateTask_ReturnsOk_WhenValid()
        {
            try
            {
                _logger.LogInfo("Starting UpdateTask_ReturnsOk_WhenValid test");
                
                // Arrange
                var task = new TaskModel { Id = "123", Title = "Updated Task" };
                
                _taskViewsMock.Setup(v => v.UpdateTaskView(It.IsAny<TaskModel>()))
                             .ReturnsAsync(new TaskModel { Id = "123", Title = "Updated Task" });

                // Act
                var response = await _client.PutAsJsonAsync("/api/v1/task", task);

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task GetOneTask_ReturnsNotFound_WhenNotExists()
        {
            try
            {
                _logger.LogInfo("Starting GetOneTask_ReturnsNotFound_WhenNotExists test");
                
                // Arrange
                var taskId = "nonexistent";
                
                _taskViewsMock.Setup(v => v.GetOneTaskView(taskId))
                             .ReturnsAsync((TaskModel?)null);

                // Act
                var response = await _client.GetAsync($"/api/v1/task/{taskId}");

                // Assert
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        public void Dispose()
        {
            _logger.LogInfo("Test cleanup");
            _client.Dispose();
            _logger.LogInfo("Resources cleaned up");
        }
    }
}