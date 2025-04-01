using Moq;
using WebApplication.Src.Controllers;
using WebApplication.Src.View;
using WebApplication.Src.Models;
using WebApplication.Src.Dto.User;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using System.Reflection;
using System.Net;
using WebApplication.Src.Config.Db;
using WebApplication.Src.Dto.user;
using WebApplication.Src.Interface;
using Microsoft.AspNetCore.Mvc.Routing;

namespace WebApplication.WebApplication.Tests.Controllers
{
    public class UserControllerTest
    {
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserController _controller;
        private readonly Mock<IUserView> _userViewsMock;

        public UserControllerTest()
        {
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockMapper = new Mock<IMapper>();
            _userViewsMock = new Mock<IUserView>();

            _controller = new UserController(
                _userViewsMock.Object,
                _mockLogger.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GetUsers_ReturnsOkResult_WithListOfUsers()
        {
            // Arrange
            var mockUsers = new List<UserModel> { new UserModel { Id = "1", Name = "Test User" } };
            var mockResponse = new List<UserResponseDTO> { 
                new UserResponseDTO { Id = "1", Name = "Test User", BirthDate = DateTime.Now, Email = "mail@mail.com" } 
            };

            _userViewsMock.Setup(x => x.GetUsers()).ReturnsAsync(mockUsers);
            _mockMapper.Setup(x => x.Map<IEnumerable<UserResponseDTO>>(mockUsers)).Returns(mockResponse);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsType<List<UserResponseDTO>>(okResult.Value);
            Assert.Single(returnedUsers);
            Assert.Equal("1", returnedUsers[0].Id);
        }
        [Fact]
        public async Task PostUser_ReturnsCreatedResult_WhenUserIsValid()
        {
            // Arrange
            var userDto = new CreateUserDTO { 
                Name = "New User", 
                Email = "test@example.com", 
                BirthDate = DateTime.Now, 
                Password = "123456" 
            };
            
            var userModel = new UserModel { 
                Id = "123",
                Name = "New User",
                Email = userDto.Email,
                BirthDate = userDto.BirthDate,
                Password = userDto.Password 
            };

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                    .Returns("api/v1/users/123");
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.Url = urlHelper.Object;

            // Setup mocks
            _mockMapper.Setup(x => x.Map<UserModel>(userDto))
                    .Returns(userModel);
            
            _userViewsMock.Setup(x => x.CreateUsers(userModel))
                        .ReturnsAsync(userModel.Id);

            var result = await _controller.PostUser(userDto);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(UserController.GetUserById), createdAtActionResult.ActionName);
            
            Assert.Equal("123", createdAtActionResult?.RouteValues?["id"]);
            
        }
        
        [Fact]
        public async Task PostUser_ReturnsConflict_WhenEmailExists()
        {
            // Arrange
            var userDto = new CreateUserDTO 
            { 
                Email = "duplicate@example.com", 
                BirthDate = DateTime.Now, 
                Password = "123456", 
                Name = "jhondoe" 
            };

            var errorDetails = new BsonDocument
            {
                { "err", "E11000 duplicate key error" },
                { "code", 11000 },
                { "keyPattern", new BsonDocument { { "email", 1 } } },
                { "keyValue", new BsonDocument { { "email", "duplicate@example.com" } } }
            };

            var instance = Activator.CreateInstance(
            typeof(WriteError),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] 
            {
                ServerErrorCategory.DuplicateKey,
                11000,
                "Duplicate key error",
                errorDetails
            },
            null
        );

            if (instance is not WriteError writeError )
            {
                throw new InvalidOperationException("Failed to create an instance of WriteError.");
                }

            var clusterId = new ClusterId(1);
            var endPoint = new DnsEndPoint("localhost", 27017);
            var serverId = new ServerId(clusterId, endPoint);
            var connectionId = new ConnectionId(serverId);

            var exception = new MongoWriteException(
                connectionId,
                writeError,
                null,
                null);

            _userViewsMock
                .Setup(x => x.CreateUsers(It.IsAny<UserModel>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _controller.PostUser(userDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            
            // Correção 1: Usar reflection para acessar as propriedades
            var responseType = conflictResult?.Value?.GetType();
            var messageProperty = responseType?.GetProperty("message");
            var fieldProperty = responseType?.GetProperty("field");

            // Correção 2: Verificar a existência das propriedades
            Assert.NotNull(messageProperty);
            Assert.NotNull(fieldProperty);

            // Correção 3: Obter os valores corretamente
            var message = messageProperty.GetValue(conflictResult?.Value)?.ToString();
            var field = fieldProperty.GetValue(conflictResult?.Value)?.ToString();

            Assert.Equal("User already exists", message);
        }
    }
}