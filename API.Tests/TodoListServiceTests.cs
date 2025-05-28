using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using API.Data;
using API.Services;
using API.DTOs;
using FeatureHubSDK;

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
            fh.Setup(f => f["enableListDeletion"].IsEnabled).Returns(flagEnabled);

            return new TodoListService(ctx, mockLogger.Object, fh.Object);
        }

        // Test: empty list when DB is empty
        [Fact]
        public async Task GetAllListsEmpty()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var result = await svc.GetAllListsAsync();

            // Assert
            Assert.Empty(result);
        }

        // Test: empty todos when new list has no items
        [Fact]
        public async Task GetTodosEmpty()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            var list = await svc.AddListAsync("Test");

            // Act
            var result = await svc.GetTodosAsync(list.Id);

            // Assert
            Assert.Empty(result);
        }

        // Test: delete non-existent list returns false
        [Fact]
        public async Task DeleteListInvalid()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var result = await svc.DeleteListAsync(123);

            // Assert
            Assert.False(result);
        }

        // Test: update non-existent todo returns null
        [Fact]
        public async Task UpdateTodoInvalid()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var result = await svc.UpdateTodoAsync(999, "text");

            // Assert
            Assert.Null(result);
        }

        // Test: delete non-existent todo returns false
        [Fact]
        public async Task DeleteTodoInvalid()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var result = await svc.DeleteTodoAsync(999);

            // Assert
            Assert.False(result);
        }

        // Test: lists are in ascending ID order
        [Fact]
        public async Task GetAllListsOrder()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            await svc.AddListAsync("A");
            await svc.AddListAsync("B");

            // Act
            var result = await svc.GetAllListsAsync();

            // Assert
            Assert.True(result[0].Id < result[1].Id);
        }

        // Test: todos are in ascending ID order
        [Fact]
        public async Task GetTodosOrder()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            var list = await svc.AddListAsync("L");
            await svc.AddTodoAsync(list.Id, "one");
            await svc.AddTodoAsync(list.Id, "two");

            // Act
            var result = await svc.GetTodosAsync(list.Id);

            // Assert
            Assert.True(result[0].Id < result[1].Id);
        }

        // Test: adding a list sets correct name
        [Fact]
        public async Task AddListCreates()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var result = await svc.AddListAsync("MyList");

            // Assert
            Assert.Equal("MyList", result.Name);
        }

        // Test: deleting a list removes it
        [Fact]
        public async Task DeleteListRemoves()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            var list = await svc.AddListAsync("ToRemove");

            // Act
            var deleted = await svc.DeleteListAsync(list.Id);

            // Assert
            Assert.True(deleted);
        }

        // Test: adding a todo sets correct text
        [Fact]
        public async Task AddTodoCreates()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            var list = await svc.AddListAsync("List");

            // Act
            var todo = await svc.AddTodoAsync(list.Id, "Task");

            // Assert
            Assert.Equal("Task", todo.Text);
        }

        // Test: updating a todo changes its text
        [Fact]
        public async Task UpdateTodoChanges()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            var list = await svc.AddListAsync("L");
            var todo = await svc.AddTodoAsync(list.Id, "old");

            // Act
            var result = await svc.UpdateTodoAsync(todo.Id, "new");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new", result!.Text);
        }

        // Test: deleting a todo removes it
        [Fact]
        public async Task DeleteTodoRemoves()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            var list = await svc.AddListAsync("L");
            var t1 = await svc.AddTodoAsync(list.Id, "first");
            await svc.AddTodoAsync(list.Id, "second");

            // Act
            var deleted = await svc.DeleteTodoAsync(t1.Id);

            // Assert
            Assert.True(deleted);
        }

        // Test: deletion disabled when feature flag is off
        [Fact]
        public async Task DeleteListFlagDisabled()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _, flagEnabled: false);
            var list = await svc.AddListAsync("F");

            // Act
            var result = await svc.DeleteListAsync(list.Id);

            // Assert
            Assert.False(result);
        }

        // Test: AddList logs Information with correct message
        [Fact]
        public async Task AddList_LogsInformation()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new TodoContext(options);
            var mockLogger = new Mock<ILogger<TodoListService>>();
            var fh = new Mock<IClientContext>();
            fh.Setup(f => f.IsSet("enableListDeletion")).Returns(true);
            fh.Setup(f => f["enableListDeletion"].IsEnabled).Returns(true);
            var svc = new TodoListService(ctx, mockLogger.Object, fh.Object);

            // Act
            await svc.AddListAsync("LoggedList");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Added new list 'LoggedList' with ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once
            );
        }

        // Test: UpdateTodo logs Warning when todo is missing
        [Fact]
        public async Task UpdateTodo_LogsWarningOnMissing()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new TodoContext(options);
            var mockLogger = new Mock<ILogger<TodoListService>>();
            var fh = new Mock<IClientContext>();
            fh.Setup(f => f.IsSet("enableListDeletion")).Returns(true);
            fh.Setup(f => f["enableListDeletion"].IsEnabled).Returns(true);
            var svc = new TodoListService(ctx, mockLogger.Object, fh.Object);

            // Act
            var result = await svc.UpdateTodoAsync(999, "text");

            // Assert
            Assert.Null(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Todo with ID 999 not found for update")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once
            );
        }
    }
}
