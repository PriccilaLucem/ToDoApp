using Moq;
using MongoDB.Driver;
using WebApplication.Src.Models;
using WebApplication.Src.View;
using WebApplication.Src.Config.Db;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebApplication.WebApplication.Tests.Model;
using WebApplication.WebApplication.Tests.Util;

namespace WebApplication.Tests.View
{
    public class UserViewsTests : IDisposable
    {
        private readonly Mock<IMongoCollection<UserModel>> _mockCollection;
        private readonly Mock<IDatabaseConfig> _mockDbConfig;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<UserViews>> _mockLogger;
        private readonly UserViews _userViews;
        private readonly TestLogger _testLogger;

        private readonly UserModel user = UserModelTest.CreateValidTestUser();

        public UserViewsTests(ITestOutputHelper output)
        {
            _testLogger = new TestLogger(output);
            
            _mockDbConfig = new Mock<IDatabaseConfig>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<UserViews>>();
            _mockCollection = new Mock<IMongoCollection<UserModel>>();

            // Setup configuration
            _mockConfig.Setup(x => x["MongoDBSettings:Collections:Users"]).Returns("Users");
            _testLogger.LogInfo("Mock configuration setup complete");

            // Setup database config to return mock collection
            _mockDbConfig.Setup(x => x.GetCollection<UserModel>("Users"))
                        .Returns(_mockCollection.Object);
            _testLogger.LogInfo("Mock database configuration setup complete");

            _userViews = new UserViews(
                _mockDbConfig.Object,
                _mockConfig.Object,
                _mockLogger.Object
            );
            _testLogger.LogInfo("UserViews instance created");
        }

        public void Dispose()
        {
            _testLogger.LogInfo("Test cleanup completed");
        }

        #region GetUserByIdAsync Tests
        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
        {
            try
            {
                _testLogger.LogInfo("Starting GetUserByIdAsync_ReturnsUser_WhenExists test");
                
                // Arrange
                var userId = "507f191e810c19729de860ea";
                var expectedUser = new UserModel { Id = userId, Name = "Test User" };
                _testLogger.LogInfo($"Testing with user ID: {userId}");

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
                _testLogger.LogInfo($"Received user with ID: {result?.Id}");

                // Assert
                Assert.NotNull(result);
                Assert.Equal(userId, result.Id);
                _testLogger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenInvalidId()
        {
            try
            {
                _testLogger.LogInfo("Starting GetUserByIdAsync_ReturnsNull_WhenInvalidId test");
                
                // Act
                var result = await _userViews.GetUserByIdAsync("invalid_id");
                _testLogger.LogInfo($"Result for invalid ID: {(result == null ? "null" : "not null")}");

                // Assert
                Assert.Null(result);
                _testLogger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task GetUserByIdAsync_Throws_WhenDatabaseFails()
        {
            try
            {
                _testLogger.LogInfo("Starting GetUserByIdAsync_Throws_WhenDatabaseFails test");
                
                // Arrange
                var userId = "507f191e810c19729de860ea";
                _testLogger.LogInfo($"Testing with user ID: {userId}");

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
                
                _testLogger.LogInfo("Test completed successfully - exception thrown as expected");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }
        #endregion

        #region GetUserByEmail Tests
        [Fact]
        public async Task GetUserByEmail_ReturnsUser_WhenExists()
        {
            try
            {
                _testLogger.LogInfo("Starting GetUserByEmail_ReturnsUser_WhenExists test");
                
                // Arrange
                var email = "john.doe@example.com";
                _testLogger.LogInfo($"Testing with email: {email}");

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
                _testLogger.LogInfo($"Received user with email: {result?.Email}");

                // Assert
                Assert.NotNull(result);
                Assert.Equal(email, result.Email);
                _testLogger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task GetUserByEmail_Throws_WhenNotFound()
        {
            try
            {
                _testLogger.LogInfo("Starting GetUserByEmail_Throws_WhenNotFound test");
                
                // Arrange
                var email = "nonexistent@example.com";
                _testLogger.LogInfo($"Testing with non-existent email: {email}");

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
                _testLogger.LogInfo("Test completed successfully - exception thrown as expected");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }
        #endregion

        #region GetUsers Tests
        [Fact]
        public async Task GetUsers_ReturnsList_WhenSuccessful()
        {
            try
            {
                _testLogger.LogInfo("Starting GetUsers_ReturnsList_WhenSuccessful test");
                
                // Arrange
                var users = new List<UserModel>
                {
                    new UserModel { Id = "1" },
                    new UserModel { Id = "2" }
                };
                _testLogger.LogInfo($"Testing with {users.Count} mock users");

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
                _testLogger.LogInfo($"Received {result.Count} users");

                // Assert
                Assert.Equal(2, result.Count);
                _testLogger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }
        #endregion

        #region CreateUsers Tests
        [Fact]
        public async Task CreateUsers_ReturnsId_WhenSuccessful()
        {
            try
            {
                _testLogger.LogInfo("Starting CreateUsers_ReturnsId_WhenSuccessful test");
                
                // Arrange
                var user = new UserModel { Email = "new@example.com" };
                _testLogger.LogInfo($"Creating user with email: {user.Email}");

                _mockCollection.Setup(x => x.InsertOneAsync(user, null, default))
                             .Returns(Task.CompletedTask);

                // Act
                var result = await _userViews.CreateUsers(user);
                _testLogger.LogInfo($"Created user with ID: {result}");

                // Assert
                Assert.NotNull(result);
                _testLogger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task CreateUsers_Throws_WhenDatabaseFails()
        {
            try
            {
                _testLogger.LogInfo("Starting CreateUsers_Throws_WhenDatabaseFails test");
                
                // Arrange
                var user = new UserModel();
                _testLogger.LogInfo("Testing with empty user model");

                _mockCollection.Setup(x => x.InsertOneAsync(user, null, default))
                             .ThrowsAsync(new MongoException("Insert failed"));

                // Act & Assert
                await Assert.ThrowsAsync<MongoException>(() => _userViews.CreateUsers(user));
                _testLogger.LogInfo("Test completed successfully - exception thrown as expected");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }
        #endregion

        #region UpdateUsers Tests
        [Fact]
        public async Task UpdateUsers_ReturnsUpdatedUser_WhenSuccessful()
        {
            try
            {
                _testLogger.LogInfo("Starting UpdateUsers_ReturnsUpdatedUser_WhenSuccessful test");
                
                // Arrange
                var user = new UserModel { Id = "1", Name = "Updated" };
                var updatedUser = new UserModel { Id = "1", Name = "Updated" };
                _testLogger.LogInfo($"Updating user with ID: {user.Id}");

                _mockCollection.Setup(x => x.FindOneAndReplaceAsync(
                    It.IsAny<FilterDefinition<UserModel>>(),
                    user,
                    It.IsAny<FindOneAndReplaceOptions<UserModel, UserModel>>(),
                    default))
                    .ReturnsAsync(updatedUser);

                // Act
                var result = await _userViews.UpdateUsers(user);
                _testLogger.LogInfo($"Updated user name: {result.Name}");

                // Assert
                Assert.Equal("Updated", result.Name);
                _testLogger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }
        #endregion

        #region DeleteUsers Tests
        [Fact]
        public async Task DeleteUsers_ReturnsTrue_WhenDeleted()
        {
            try
            {
                _testLogger.LogInfo("Starting DeleteUsers_ReturnsTrue_WhenDeleted test");
                
                // Arrange
                var id = "507f191e810c19729de860ea";
                _testLogger.LogInfo($"Deleting user with ID: {id}");

                var deleteResult = new DeleteResult.Acknowledged(1);
                
                _mockCollection.Setup(x => x.DeleteOneAsync(
                    It.IsAny<FilterDefinition<UserModel>>(),
                    default))
                    .ReturnsAsync(deleteResult);

                // Act
                var result = await _userViews.DeleteUsers(id);
                _testLogger.LogInfo($"Delete result: {result}");

                // Assert
                Assert.True(result);
                _testLogger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task DeleteUsers_ReturnsFalse_WhenInvalidId()
        {
            try
            {
                _testLogger.LogInfo("Starting DeleteUsers_ReturnsFalse_WhenInvalidId test");
                
                // Act
                var result = await _userViews.DeleteUsers("invalid_id");
                _testLogger.LogInfo($"Delete result for invalid ID: {result}");

                // Assert
                Assert.False(result);
                _testLogger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _testLogger.LogError("Test failed", ex);
                throw;
            }
        }
        #endregion
    }
}