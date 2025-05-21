using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using API.Data;
using API.Models;
using API.Services;

namespace API.Tests
{
    public class TodoListServiceCoreTests
    {
        private static TodoListService CreateService(string dbName)
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new TodoContext(options);
            return new TodoListService(ctx);
        }

        [Fact]
        public async Task AddListAsync_Should_Return_New_List_And_Persist()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var list = await svc.AddListAsync("Groceries");

            // Assert
            Assert.NotNull(list);
            Assert.Equal("Groceries", list.Name);
            var all = await svc.GetAllListsAsync();
            Assert.Single(all);
            Assert.Equal(list.Id, all[0].Id);
        }

        [Fact]
        public async Task DeleteListAsync_Should_Remove_List_And_Its_Todos()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Work");
            await svc.AddTodoAsync(list.Id, "Email client");

            // Act
            var result = await svc.DeleteListAsync(list.Id);

            // Assert
            Assert.True(result);
            Assert.Empty(await svc.GetAllListsAsync());
            Assert.Empty(await svc.GetTodosAsync(list.Id));
        }

        [Fact]
        public async Task AddTodoAsync_Should_Create_Todo_In_Specified_List()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Errands");

            // Act
            var todo = await svc.AddTodoAsync(list.Id, "Buy milk");

            // Assert
            Assert.NotNull(todo);
            Assert.Equal(list.Id, todo.TodoListId);
            var todos = await svc.GetTodosAsync(list.Id);
            Assert.Single(todos);
            Assert.Equal(todo.Id, todos[0].Id);
        }

        [Fact]
        public async Task UpdateTodoAsync_Should_Change_Existing_Todo_Text()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Reading");
            var todo = await svc.AddTodoAsync(list.Id, "Old title");

            // Act
            var updated = await svc.UpdateTodoAsync(todo.Id, "New title");

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("New title", updated.Text);
            Assert.Equal("New title", (await svc.GetTodosAsync(list.Id)).First().Text);
        }

        [Fact]
        public async Task DeleteTodoAsync_Should_Remove_Only_That_Todo()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Chores");
            var t1 = await svc.AddTodoAsync(list.Id, "Clean");
            var t2 = await svc.AddTodoAsync(list.Id, "Cook");

            // Act
            var deleted = await svc.DeleteTodoAsync(t1.Id);

            // Assert
            Assert.True(deleted);
            var remaining = await svc.GetTodosAsync(list.Id);
            Assert.Single(remaining);
            Assert.Equal(t2.Id, remaining[0].Id);
        }
    }
}
