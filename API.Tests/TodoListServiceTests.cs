using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using API.Data;
using API.Services;
using API.DTOs;
using FeatureHubSDK;
using System;
using System.Threading.Tasks;

namespace API.Tests
{
    public class TodoListServiceTests
    {
        // Helper: create service with in-memory DB, mock logger, and feature flag
        private static TodoListService CreateService(string dbName, out Mock<ILogger<TodoListService>> mockLogger, bool flagEnabled = true)
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new TodoContext(options);

            mockLogger = new Mock<ILogger<TodoListService>>();
            var fh = new Mock<IClientContext>();
            fh.Setup(f => f.IsSet("enableListDeletion")).Returns(flagEnabled);
            var ffFlag = new Mock<IFeature>();
            ffFlag.Setup(f => f.IsEnabled).Returns(flagEnabled);
            fh.Setup(f => f["enableListDeletion"]).Returns(ffFlag.Object);

            return new TodoListService(ctx, mockLogger.Object, fh.Object);
        }

        // Test: AddList saves and logs correctly
        [Fact]
        public async Task AddList_SavesAndLogs()
        {
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out var logger);

            var result = await svc.AddListAsync("New List");

            Assert.Equal("New List", result.Name);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("Added new list")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: DeleteList returns false and logs warning if flag disabled
        [Fact]
        public async Task DeleteList_FlagDisabled_ReturnsFalseAndLogs()
        {
            var svc = CreateService(Guid.NewGuid().ToString(), out var logger, flagEnabled: false);
            var result = await svc.DeleteListAsync(123);
            Assert.False(result);

            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("List deletion feature is disabled")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: DeleteList returns false and logs warning if list not found
        [Fact]
        public async Task DeleteList_NotFound_ReturnsFalseAndLogs()
        {
            var svc = CreateService(Guid.NewGuid().ToString(), out var logger);
            var result = await svc.DeleteListAsync(999);
            Assert.False(result);

            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("No list was found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: DeleteList success logs info
        [Fact]
        public async Task DeleteList_Success_DeletesAndLogs()
        {
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out var logger);
            var list = await svc.AddListAsync("DeleteMe");

            var deleted = await svc.DeleteListAsync(list.Id);
            Assert.True(deleted);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("Successfully deleted")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: AddTodo creates and logs info
        [Fact]
        public async Task AddTodo_CreatesAndLogs()
        {
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out var logger);
            var list = await svc.AddListAsync("List");

            var todo = await svc.AddTodoAsync(list.Id, "Task");

            Assert.Equal("Task", todo.Text);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("Added todo")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: UpdateTodo updates and logs info
        [Fact]
        public async Task UpdateTodo_UpdatesAndLogs()
        {
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out var logger);
            var list = await svc.AddListAsync("L");
            var todo = await svc.AddTodoAsync(list.Id, "old");

            var updated = await svc.UpdateTodoAsync(todo.Id, "new");

            Assert.NotNull(updated);
            Assert.Equal("new", updated!.Text);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("Updated todo")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: UpdateTodo returns null and logs warning if not found
        [Fact]
        public async Task UpdateTodo_ReturnsNullAndLogsWarning()
        {
            var svc = CreateService(Guid.NewGuid().ToString(), out var logger);
            var result = await svc.UpdateTodoAsync(999, "text");
            Assert.Null(result);

            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("not found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: DeleteTodo removes and logs info
        [Fact]
        public async Task DeleteTodo_DeletesAndLogs()
        {
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out var logger);
            var list = await svc.AddListAsync("L");
            var todo = await svc.AddTodoAsync(list.Id, "toDelete");

            var deleted = await svc.DeleteTodoAsync(todo.Id);
            Assert.True(deleted);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("Deleted todo")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: DeleteTodo returns false and logs warning if not found
        [Fact]
        public async Task DeleteTodo_ReturnsFalseAndLogsWarning()
        {
            var svc = CreateService(Guid.NewGuid().ToString(), out var logger);
            var deleted = await svc.DeleteTodoAsync(999);
            Assert.False(deleted);

            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains("not found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        // Test: GetTodos returns todos and logs info
        [Fact]
        public async Task GetTodos_ReturnsTodosAndLogs()
        {
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out var logger);
            var list = await svc.AddListAsync("L");
            await svc.AddTodoAsync(list.Id, "Task1");
            await svc.AddTodoAsync(list.Id, "Task2");

            var todos = await svc.GetTodosAsync(list.Id);

            Assert.Equal(2, todos.Count);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>((v, t) => v != null && v.ToString()!.Contains($"Fetching todos for list ID {list.Id}")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }
    }
}
