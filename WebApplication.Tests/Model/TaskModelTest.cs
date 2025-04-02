using System;
using System.Collections.Generic;
using WebApplication.Src.Models.TaskModel;
using Xunit;

namespace WebApplication.WebApplication.Tests.Model
{
    public class TaskModelTest
    {
        [Fact]
        public void TaskModel_Should_Have_Default_Values()
        {
            var task = new TaskModel();

            Assert.NotNull(task.Id);
            Assert.Equal(string.Empty, task.Title);
            Assert.Equal(string.Empty, task.Description);
            Assert.Equal("Personal Care", task.Category);
            Assert.Null(task.DueDate);
            Assert.Equal(3, task.Priority);
            Assert.False(task.IsCompleted);
            Assert.Equal(15, task.DurationMinutes);
            Assert.InRange(task.CreatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            Assert.InRange(task.UpdatedAt, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            Assert.NotNull(task.Tags);
            Assert.Empty(task.Tags);
            Assert.Null(task.Recurrence);
            Assert.Equal(string.Empty, task.UserId);
        }

        [Fact]
        public void TaskModel_Should_Allow_Setting_Values()
        {
            var now = DateTime.UtcNow;
            var task = new TaskModel
            {
                Id = "1234567890abcdef",
                Title = "Test Task",
                Description = "This is a test description",
                Category = "Work",
                DueDate = now.AddDays(1),
                Priority = 1,
                IsCompleted = true,
                DurationMinutes = 45,
                CreatedAt = now,
                UpdatedAt = now,
                Tags = new List<string> { "urgent", "important" },
                UserId = "abcdef1234567890"
            };

            Assert.Equal("1234567890abcdef", task.Id);
            Assert.Equal("Test Task", task.Title);
            Assert.Equal("This is a test description", task.Description);
            Assert.Equal("Work", task.Category);
            Assert.Equal(now.AddDays(1), task.DueDate);
            Assert.Equal(1, task.Priority);
            Assert.True(task.IsCompleted);
            Assert.Equal(45, task.DurationMinutes);
            Assert.Equal(now, task.CreatedAt);
            Assert.Equal(now, task.UpdatedAt);
            Assert.Contains("urgent", task.Tags);
            Assert.Contains("important", task.Tags);
            Assert.Equal("abcdef1234567890", task.UserId);
        }
    }
}