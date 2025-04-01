using Moq;
using MongoDB.Driver;
using WebApplication.Src.Models;
using WebApplication.Src.View;
using WebApplication.Src.Config.Db;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;
using WebApplication.WebApplication.Tests.Model;

namespace WebApplication.Tests.View
{
    public class UserViewsTests
    {
        private readonly Mock<IMongoCollection<UserModel>> _mockCollection;
        private readonly Mock<IDatabaseConfig> _mockDbConfig;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<UserViews>> _mockLogger;
        private readonly UserViews _userViews;

        private readonly UserModel user =   UserModelTest.CreateValidTestUser();
        public UserViewsTests()
        {
            _mockDbConfig = new Mock<IDatabaseConfig>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<UserViews>>();
            _mockCollection = new Mock<IMongoCollection<UserModel>>();

            // Setup configuration
            _mockConfig.Setup(x => x["MongoDBSettings:Collections:Users"]).Returns("Users");

            // Setup database config to return mock collection
            _mockDbConfig.Setup(x => x.GetCollection<UserModel>("Users"))
                        .Returns(_mockCollection.Object);

            _userViews = new UserViews(
                _mockDbConfig.Object,
                _mockConfig.Object,
                _mockLogger.Object
            );
        }

        #region GetUserByIdAsync Tests
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
        {
            // Arrange
            var userId = "507f191e810c19729de860ea";
            var expectedUser = new UserModel { Id = userId, Name = "Test User" };

            var mockCursor = new Mock<IAsyncCursor<UserModel>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(default))
                     .ReturnsAsync(true)
                     .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns(new[] { expectedUser });

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<UserModel>>(),
                It.IsAny<FindOptions<UserModel, UserModel>>(),
                default))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _userViews.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenInvalidId()
        {
            // Act
            var result = await _userViews.GetUserByIdAsync("invalid_id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_Throws_WhenDatabaseFails()
        {
            // Arrange
            var userId = "507f191e810c19729de860ea";
            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<UserModel>>(),
                It.IsAny<FindOptions<UserModel, UserModel>>(),
                default))
                .ThrowsAsync(new MongoException("Connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<MongoException>(() => _userViews.GetUserByIdAsync(userId));
            
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains($"Error getting user with id {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),                
                Times.Once);
        }
        #endregion

        #region GetUserByEmail Tests
        [Fact]
        public async Task GetUserByEmail_ReturnsUser_WhenExists()
        {
            // Arrange
            var email = "john.doe@example.com";


            var mockCursor = new Mock<IAsyncCursor<UserModel>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(default))
                     .ReturnsAsync(true)
                     .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns([user]);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<UserModel>>(),
                It.IsAny<FindOptions<UserModel, UserModel>>(),
                default))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _userViews.GetUserByEmail(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetUserByEmail_Throws_WhenNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var mockCursor = new Mock<IAsyncCursor<UserModel>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(default))
                     .ReturnsAsync(false);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<UserModel>>(),
                It.IsAny<FindOptions<UserModel, UserModel>>(),
                default))
                .ReturnsAsync(mockCursor.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userViews.GetUserByEmail(email));
        }
        #endregion

        #region GetUsers Tests
        [Fact]
        public async Task GetUsers_ReturnsList_WhenSuccessful()
        {
            // Arrange
            var users = new List<UserModel>
            {
                new UserModel { Id = "1" },
                new UserModel { Id = "2" }
            };

            var mockCursor = new Mock<IAsyncCursor<UserModel>>();
            mockCursor.SetupSequence(x => x.MoveNextAsync(default))
                     .ReturnsAsync(true)
                     .ReturnsAsync(false);
            mockCursor.Setup(x => x.Current).Returns(users);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<UserModel>>(),
                It.IsAny<FindOptions<UserModel, UserModel>>(),
                default))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _userViews.GetUsers();

            // Assert
            Assert.Equal(2, result.Count);
        }
        #endregion

        #region CreateUsers Tests
        [Fact]
        public async Task CreateUsers_ReturnsId_WhenSuccessful()
        {
            // Arrange
            var user = new UserModel { Email = "new@example.com" };
            _mockCollection.Setup(x => x.InsertOneAsync(user, null, default))
                         .Returns(Task.CompletedTask);

            // Act
            var result = await _userViews.CreateUsers(user);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateUsers_Throws_WhenDatabaseFails()
        {
            // Arrange
            var user = new UserModel();
            _mockCollection.Setup(x => x.InsertOneAsync(user, null, default))
                         .ThrowsAsync(new MongoException("Insert failed"));

            await Assert.ThrowsAsync<MongoException>(() => _userViews.CreateUsers(user));
        }
        #endregion

        #region UpdateUsers Tests
        [Fact]
        public async Task UpdateUsers_ReturnsUpdatedUser_WhenSuccessful()
        {
            // Arrange
            var user = new UserModel { Id = "1", Name = "Updated" };
            var updatedUser = new UserModel { Id = "1", Name = "Updated" };

            _mockCollection.Setup(x => x.FindOneAndReplaceAsync(
                It.IsAny<FilterDefinition<UserModel>>(),
                user,
                It.IsAny<FindOneAndReplaceOptions<UserModel, UserModel>>(),
                default))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _userViews.UpdateUsers(user);

            // Assert
            Assert.Equal("Updated", result.Name);
        }

        #endregion

        #region DeleteUsers Tests
        [Fact]
        public async Task DeleteUsers_ReturnsTrue_WhenDeleted()
        {
            // Arrange
            var id = "507f191e810c19729de860ea";
            var deleteResult = new DeleteResult.Acknowledged(1);
            
            _mockCollection.Setup(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<UserModel>>(),
                default))
                .ReturnsAsync(deleteResult);

            // Act
            var result = await _userViews.DeleteUsers(id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUsers_ReturnsFalse_WhenInvalidId()
        {
            // Act
            var result = await _userViews.DeleteUsers("invalid_id");

            // Assert
            Assert.False(result);
        }
        #endregion
    }
}