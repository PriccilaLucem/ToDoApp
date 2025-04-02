using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication.Src.Controllers;
using WebApplication.Src.Dto.task;
using WebApplication.Src.Models.TaskModel;
using WebApplication.Src.View;
using Xunit;
using AutoMapper;
using WebApplication.Src.Interface;
using Xunit.Abstractions;
using WebApplication.WebApplication.Tests.Util;
using Microsoft.AspNetCore.Mvc.Routing;
using WebApplication.WebApplication.Tests.Interface;
using Microsoft.AspNetCore.Http;

namespace WebApplication.WebApplication.Tests.Controllers
{
    public class TaskControllerTest : IDisposable
    {
        private readonly Mock<IFakeTaskView> _mockTaskViews;
        private readonly Mock<ILogger<TaskController>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly TaskController _controller;
        private readonly TestLogger _testLogger;

        public TaskControllerTest(ITestOutputHelper output)
        {
            _testLogger = new TestLogger(output);
            _mockTaskViews = new Mock<IFakeTaskView>();    
            _mockLogger = new Mock<ILogger<TaskController>>();
            _mockMapper = new Mock<IMapper>();
            
            // Initialize controller with mocked dependencies
            _controller = new TaskController(
                _mockLogger.Object, 
                _mockTaskViews.Object, 
                _mockMapper.Object
            );
            
            _testLogger.LogInfo("Test initialized - mocks and controller created");
            
        }

        public void Dispose()
        {
            _testLogger.LogInfo("Test cleanup completed");
        }

       [Fact]
public async Task PostTask_ReturnsCreatedResult_WhenTaskIsValid()
{
    try
    {
        _testLogger.LogInfo("Starting PostTask_ReturnsCreatedResult_WhenTaskIsValid test");

        var taskDto = new TaskDTO
        {
            Title = "Test Task",
            Description = "This is a test task",
            DueDate = DateTime.Now.AddDays(1)
        };

        var taskModel = new TaskModel
        {
            Id = "123",
            Title = taskDto.Title,
            Description = taskDto.Description,
            DueDate = taskDto.DueDate
        };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                 .Returns("api/v1/task/123");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.Url = urlHelper.Object;

        _mockMapper.Setup(x => x.Map<TaskModel>(taskDto))
                   .Returns(taskModel);

        _mockTaskViews.Setup(x => x.CreateTaskView(taskModel))
                          .ReturnsAsync(taskModel.Id);

        var result = await _controller.PostTask(taskDto);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(TaskController.GetOneTask), createdAtActionResult.ActionName);

        _testLogger.LogInfo("Test completed successfully");
    }
    catch (Exception ex)
    {
        _testLogger.LogError("Test failed", ex);
        throw;
    }
    }
    }
}