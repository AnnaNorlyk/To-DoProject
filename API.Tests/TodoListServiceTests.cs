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
            var ffFlag = new Mock<IFeature>();
            ffFlag.Setup(f => f.IsEnabled).Returns(flagEnabled);
            fh.Setup(f => f["enableListDeletion"]).Returns(ffFlag.Object);

            return new TodoListService(ctx, mockLogger.Object, fh.Object);
        }

        // Test: empty list when DB is empty
        [Fact]
        public async Task GetAllLists_ReturnsEmpty_WhenNoListsExist()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var result = await svc.GetAllListsAsync();

            // Assert
            Assert.Empty(result);
        }

        // Test: GetAllLists returns all lists with correct order and todos
        [Fact]
        public async Task GetAllLists_ReturnsListsWithTodos_InOrder()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out _);
            var listA = await svc.AddListAsync("A");
            var listB = await svc.AddListAsync("B");
            await svc.AddTodoAsync(listA.Id, "todo1");
            await svc.AddTodoAsync(listA.Id, "todo2");
            await svc.AddTodoAsync(listB.Id, "todo3");

            // Act
            var results = await svc.GetAllListsAsync();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.True(results[0].Id < results[1].Id);
            Assert.Equal(listA.Id, results[0].Id);
            Assert.Equal(2, results[0].Todos.Count);
            Assert.Equal("todo1", results[0].Todos[0].Text);
            Assert.Equal("todo2", results[0].Todos[1].Text);
            Assert.Single(results[1].Todos);
            Assert.Equal("todo3", results[1].Todos[0].Text);
        }

        // Test: empty todos when new list has no todos
        [Fact]
        public async Task GetTodos_ReturnsEmpty_WhenNoTodosExist()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            var list = await svc.AddListAsync("Test");

            // Act
            var result = await svc.GetTodosAsync(list.Id);

            // Assert
            Assert.Empty(result);
        }

        // Test: GetTodos returns todos in ascending order for list
        [Fact]
        public async Task GetTodos_ReturnsTodos_InOrder()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);
            var list = await svc.AddListAsync("List");
            await svc.AddTodoAsync(list.Id, "one");
            await svc.AddTodoAsync(list.Id, "two");

            // Act
            var todos = await svc.GetTodosAsync(list.Id);

            // Assert
            Assert.Equal(2, todos.Count);
            Assert.True(todos[0].Id < todos[1].Id);
        }

        // Test: AddList creates and persists a list
        [Fact]
        public async Task AddList_CreatesAndPersistsList()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out _);

            // Act
            var dto = await svc.AddListAsync("MyList");

            // Assert
            Assert.Equal("MyList", dto.Name);
            // Verify it is actually saved in DB
            using var ctx = new TodoContext(new DbContextOptionsBuilder<TodoContext>().UseInMemoryDatabase(dbName).Options);
            var found = await ctx.TodoLists.FindAsync(dto.Id);
            Assert.NotNull(found);
            Assert.Equal("MyList", found!.Name);
        }

        // Test: AddTodo creates and persists todo linked to list
        [Fact]
        public async Task AddTodo_CreatesAndPersistsTodo()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out _);
            var list = await svc.AddListAsync("List");

            // Act
            var todo = await svc.AddTodoAsync(list.Id, "Task");

            // Assert
            Assert.Equal("Task", todo.Text);
            // Verify saved in DB and linked
            using var ctx = new TodoContext(new DbContextOptionsBuilder<TodoContext>().UseInMemoryDatabase(dbName).Options);
            var found = await ctx.Todos.FindAsync(todo.Id);
            Assert.NotNull(found);
            Assert.Equal("Task", found!.Text);
            Assert.Equal(list.Id, found.TodoListId);
        }

        // Test: UpdateTodo updates text for existing todo
        [Fact]
        public async Task UpdateTodo_UpdatesText_WhenTodoExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out _);
            var list = await svc.AddListAsync("L");
            var todo = await svc.AddTodoAsync(list.Id, "old");

            // Act
            var updated = await svc.UpdateTodoAsync(todo.Id, "new");

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("new", updated!.Text);
            // Verify DB updated
            using var ctx = new TodoContext(new DbContextOptionsBuilder<TodoContext>().UseInMemoryDatabase(dbName).Options);
            var found = await ctx.Todos.FindAsync(todo.Id);
            Assert.NotNull(found);
            Assert.Equal("new", found!.Text);
        }

        // Test: UpdateTodo returns null if todo not found
        [Fact]
        public async Task UpdateTodo_ReturnsNull_WhenTodoNotFound()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var result = await svc.UpdateTodoAsync(999, "text");

            // Assert
            Assert.Null(result);
        }

        // Test: DeleteTodo removes existing todo and returns true
        [Fact]
        public async Task DeleteTodo_RemovesTodo_WhenExists()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out _);
            var list = await svc.AddListAsync("L");
            var todo = await svc.AddTodoAsync(list.Id, "toDelete");

            // Act
            var deleted = await svc.DeleteTodoAsync(todo.Id);

            // Assert
            Assert.True(deleted);
            // Verify removed from DB
            using var ctx = new TodoContext(new DbContextOptionsBuilder<TodoContext>().UseInMemoryDatabase(dbName).Options);
            var found = await ctx.Todos.FindAsync(todo.Id);
            Assert.Null(found);
        }

        // Test: DeleteTodo returns false if todo not found
        [Fact]
        public async Task DeleteTodo_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var deleted = await svc.DeleteTodoAsync(999);

            // Assert
            Assert.False(deleted);
        }

        // Test: DeleteList removes list when feature flag enabled
        [Fact]
        public async Task DeleteList_RemovesList_WhenFeatureFlagEnabled()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var svc = CreateService(dbName, out _);
            var list = await svc.AddListAsync("ToRemove");

            // Act
            var deleted = await svc.DeleteListAsync(list.Id);

            // Assert
            Assert.True(deleted);
            // Verify removed from DB
            using var ctx = new TodoContext(new DbContextOptionsBuilder<TodoContext>().UseInMemoryDatabase(dbName).Options);
            var found = await ctx.TodoLists.FindAsync(list.Id);
            Assert.Null(found);
        }

        // Test: DeleteList returns false when feature flag disabled
        [Fact]
        public async Task DeleteList_ReturnsFalse_WhenFeatureFlagDisabled()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _, flagEnabled: false);
            var list = await svc.AddListAsync("F");

            // Act
            var result = await svc.DeleteListAsync(list.Id);

            // Assert
            Assert.False(result);
        }

        // Test: DeleteList returns false if list not found
        [Fact]
        public async Task DeleteList_ReturnsFalse_WhenListNotFound()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString(), out _);

            // Act
            var result = await svc.DeleteListAsync(999);

            // Assert
            Assert.False(result);
        }

        // Logging tests omitted here for brevity but should remain as in your original file
    }
}
