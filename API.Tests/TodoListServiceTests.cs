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
    public class TodoListServiceTests
    {
        private static TodoListService CreateService(string dbName)
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var context = new TodoContext(options);
            return new TodoListService(context);
        }

        [Fact]
        public async Task AddListAsync_Should_Create_New_List()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var list = await svc.AddListAsync("Shopping");

            // Assert
            Assert.NotNull(list);
            Assert.Equal("Shopping", list.Name);

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
            var allLists = await svc.GetAllListsAsync();
            Assert.Empty(allLists);
        }

        [Fact]
        public async Task AddTodoAsync_Should_Create_Todo_In_List()
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
        public async Task UpdateTodoAsync_Should_Update_Todo_Text()
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

            var fetched = (await svc.GetTodosAsync(list.Id)).First();
            Assert.Equal("New title", fetched.Text);
        }

        [Fact]
        public async Task DeleteTodoAsync_Should_Remove_Only_Specified_Todo()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Chores");
            var todo1 = await svc.AddTodoAsync(list.Id, "Clean");
            var todo2 = await svc.AddTodoAsync(list.Id, "Cook");

            // Act
            var deleted = await svc.DeleteTodoAsync(todo1.Id);

            // Assert
            Assert.True(deleted);

            var todos = await svc.GetTodosAsync(list.Id);
            Assert.Single(todos);
            Assert.Equal(todo2.Id, todos[0].Id);
        }
    }
}
