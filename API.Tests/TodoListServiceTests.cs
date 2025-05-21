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
        public async Task GetAllListsAsync_OnEmptyDb_ReturnsEmpty()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var all = await svc.GetAllListsAsync();

            // Assert
            Assert.Empty(all);
        }

        [Fact]
        public async Task GetTodosAsync_OnEmptyList_ReturnsEmpty()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Whatever");

            // Act
            var todos = await svc.GetTodosAsync(list.Id);

            // Assert
            Assert.Empty(todos);
        }

        [Fact]
        public async Task DeleteListAsync_InvalidId_ReturnsFalse()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var result = await svc.DeleteListAsync(1234);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateTodoAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var updated = await svc.UpdateTodoAsync(999, "nope");

            // Assert
            Assert.Null(updated);
        }

        [Fact]
        public async Task DeleteTodoAsync_InvalidId_ReturnsFalse()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());

            // Act
            var deleted = await svc.DeleteTodoAsync(999);

            // Assert
            Assert.False(deleted);
        }

        [Fact]
        public async Task GetAllListsAsync_ReturnsLists_InIdOrder()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var l1 = await svc.AddListAsync("First");
            var l2 = await svc.AddListAsync("Second");

            // Act
            var all = await svc.GetAllListsAsync();

            // Assert
            Assert.Equal(new[] { l1.Id, l2.Id }, all.Select(x => x.Id));
        }

        [Fact]
        public async Task GetTodosAsync_ReturnsTodos_InIdOrder()
        {
            // Arrange
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("L");
            var t1 = await svc.AddTodoAsync(list.Id, "one");
            var t2 = await svc.AddTodoAsync(list.Id, "two");

            // Act
            var todos = await svc.GetTodosAsync(list.Id);

            // Assert
            Assert.Equal(new[] { t1.Id, t2.Id }, todos.Select(x => x.Id));
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
            await svc.AddTodoAsync(list.Id, "Email");

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
            var todo = await svc.AddTodoAsync(list.Id, "Old");

            // Act
            var updated = await svc.UpdateTodoAsync(todo.Id, "New");

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("New", updated.Text);
            var fetched = (await svc.GetTodosAsync(list.Id)).First();
            Assert.Equal("New", fetched.Text);
        }

        [Fact]
        public async Task DeleteTodoAsync_Should_Remove_Only_Specified_Todo()
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
            var todos = await svc.GetTodosAsync(list.Id);
            Assert.Single(todos);
            Assert.Equal(t2.Id, todos[0].Id);
        }
    }
}
