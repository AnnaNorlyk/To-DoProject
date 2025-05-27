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
        private static TodoListService CreateService(string dbName, bool flagEnabled = true)
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new TodoContext(options);
            var logger = new Mock<ILogger<TodoListService>>();
            var fh = new Mock<IClientContext>();
            fh.Setup(f => f.IsSet("enableListDeletion")).Returns(flagEnabled);
            fh.Setup(f => f["enableListDeletion"].IsEnabled).Returns(flagEnabled);
            return new TodoListService(ctx, logger.Object, fh.Object);
        }

        // Check empty list returned when database is empty
        [Fact]
        public async Task GetAllListsEmpty()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var result = await svc.GetAllListsAsync();

            // Assert
            Assert.Empty(result);
        }

        // Check empty todos when list has no items
        [Fact]
        public async Task GetTodosEmpty()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Test");

            // Act
            var result = await svc.GetTodosAsync(list.Id);

            // Assert
            Assert.Empty(result);
        }

        // Check deletion returns false for non-existent list
        [Fact]
        public async Task DeleteListInvalid()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var result = await svc.DeleteListAsync(123);

            // Assert
            Assert.False(result);
        }

        // Check null returned when updating non-existent todo
        [Fact]
        public async Task UpdateTodoInvalid()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var result = await svc.UpdateTodoAsync(999, "text");

            // Assert
            Assert.Null(result);
        }

        // Check deletion returns false for non-existent todo
        [Fact]
        public async Task DeleteTodoInvalid()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var result = await svc.DeleteTodoAsync(999);

            // Assert
            Assert.False(result);
        }

        // Check lists are returned in ascending ID order
        [Fact]
        public async Task GetAllListsOrder()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            await svc.AddListAsync("A");
            await svc.AddListAsync("B");

            // Act
            var result = await svc.GetAllListsAsync();

            // Assert
            Assert.True(result[0].Id < result[1].Id);
        }

        // Check todos are returned in ascending ID order
        [Fact]
        public async Task GetTodosOrder()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("L");
            await svc.AddTodoAsync(list.Id, "one");
            await svc.AddTodoAsync(list.Id, "two");

            // Act
            var result = await svc.GetTodosAsync(list.Id);

            // Assert
            Assert.True(result[0].Id < result[1].Id);
        }

        // Check new list is created with correct name
        [Fact]
        public async Task AddListCreates()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var result = await svc.AddListAsync("MyList");

            // Assert
            Assert.Equal("MyList", result.Name);
        }

        // Check list deletion removes list
        [Fact]
        public async Task DeleteListRemoves()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("ToRemove");

            // Act
            var deleted = await svc.DeleteListAsync(list.Id);

            // Assert
            Assert.True(deleted);
        }

        // Check new todo is added to list
        [Fact]
        public async Task AddTodoCreates()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("List");

            // Act
            var todo = await svc.AddTodoAsync(list.Id, "Task");

            // Assert
            Assert.Equal("Task", todo.Text);
        }

        // Check todo text is updated correctly
        [Fact]
        public async Task UpdateTodoChanges()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("L");
            var todo = await svc.AddTodoAsync(list.Id, "old");

            // Act
            var result = await svc.UpdateTodoAsync(todo.Id, "new");

            // Assert
            Assert.Equal("new", result.Text);
        }

        // Check specified todo is deleted
        [Fact]
        public async Task DeleteTodoRemoves()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("L");
            var t1 = await svc.AddTodoAsync(list.Id, "first");
            await svc.AddTodoAsync(list.Id, "second");

            // Act
            var deleted = await svc.DeleteTodoAsync(t1.Id);

            // Assert
            Assert.True(deleted);
        }

        // Check deletion disabled when feature flag off
        [Fact]
        public async Task DeleteListFlagDisabled()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), flagEnabled: false);
            var list = await svc.AddListAsync("F");

            // Act
            var result = await svc.DeleteListAsync(list.Id);

            // Assert
            Assert.False(result);
        }
    }
}
