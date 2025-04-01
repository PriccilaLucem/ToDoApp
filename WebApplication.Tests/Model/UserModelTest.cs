using System.ComponentModel.DataAnnotations;
using WebApplication.Src.Models; // Adjust namespace as needed
using MongoDB.Bson;
using Xunit;

namespace WebApplication.WebApplication.Tests.Model
{
    public class UserModelTest
    {
        // Helper method to create a valid test user
        public static UserModel CreateValidTestUser()
        {
            return new UserModel
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = "SecurePassword123!",
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                Status = true
            };
        }
        public static UserModel CreateInvalidTestUser()
        {
            return new UserModel
            {
                Id = "string",
                Name = null,
                Email = "john.doe",
                Password = "",
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                Status = true
            };
        }

        [Fact]
        public void UserModel_ValidData_PassesValidation()
        {
            // Arrange
            var user = CreateValidTestUser();
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UserModel_MissingName_FailsValidation(string name)
        {
            // Arrange
            var user = CreateValidTestUser();
            user.Name = name;
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(UserModel.Name)));
        }

        [Fact]
        public void UserModel_InvalidBirthDate_FailsValidation()
        {
            var user = CreateValidTestUser();
            user.BirthDate = DateTime.Now.AddDays(1); // Future date
            
            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            Assert.False(isValid);
            Assert.Contains(validationResults, 
                v => v.MemberNames.Any(m => m == nameof(UserModel.BirthDate)));
        }
        [Fact]
        public void UserModel_ObjectIdConversion_WorksCorrectly()
        {
            var user = CreateValidTestUser();
            var objectIdString = user.Id;

            var isValidObjectId = ObjectId.TryParse(objectIdString, out _);

            Assert.True(isValidObjectId);
        }

        [Fact]
        public void UserModel_DefaultValues_AreSetCorrectly()
        {
            var user = new UserModel();

            Assert.NotNull(user.Id);
            Assert.True(ObjectId.TryParse(user.Id, out _));
            Assert.Equal(DateTime.UtcNow.Date, user.CreatedAt.Date);
            Assert.Equal(DateTime.UtcNow.Date, user.UpdatedAt.Date);
        }

        [Fact]
        public void UserModel_Collections_AreInitialized()
        {
            var user = new UserModel();

            Assert.NotNull(user.Tags);
            Assert.Empty(user.Tags);
        }
    }
}