using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using API.Data;
using API.Models;
using API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests
{
    public class TodoContextTests
    {
        private static TodoListService CreateService(string dbName)
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var ctx = new TodoContext(options);
            var logger = new Mock<ILogger<TodoListService>>();
            return new TodoListService(ctx, logger.Object);
        }

        [Fact]
        public async Task AddListAsync_Should_Return_New_List_And_Persist()
        {
            var svc = CreateService(Guid.NewGuid().ToString());

            var result = await svc.AddListAsync("Groceries");

            var all = await svc.GetAllListsAsync();
            Assert.NotNull(result);
            Assert.Equal("Groceries", result.Name);
            Assert.Single(all);
            Assert.Equal(result.Id, all[0].Id);
        }

        [Fact]
        public async Task DeleteListAsync_Should_Remove_List_And_Its_Todos()
        {
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Work");
            await svc.AddTodoAsync(list.Id, "Email client");

            var result = await svc.DeleteListAsync(list.Id);

            var lists = await svc.GetAllListsAsync();
            var todos = await svc.GetTodosAsync(list.Id);
            Assert.True(result);
            Assert.Empty(lists);
            Assert.Empty(todos);
        }

        [Fact]
        public async Task AddTodoAsync_Should_Create_Todo_In_Specified_List()
        {
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Errands");

            var todo = await svc.AddTodoAsync(list.Id, "Buy milk");

            var todos = await svc.GetTodosAsync(list.Id);
            Assert.NotNull(todo);
            Assert.Equal("Buy milk", todo.Text);

            Assert.Single(todos);
        }

        [Fact]
        public async Task UpdateTodoAsync_Should_Change_Existing_Todo_Text()
        {
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Reading");
            var todo = await svc.AddTodoAsync(list.Id, "Old title");

            var updated = await svc.UpdateTodoAsync(todo.Id, "New title");

            var todos = await svc.GetTodosAsync(list.Id);
            Assert.NotNull(updated);
            Assert.Equal("New title", updated.Text);
            Assert.Equal("New title", todos.First().Text);
        }

        [Fact]
        public async Task DeleteTodoAsync_Should_Remove_Only_That_Todo()
        {
            var svc = CreateService(Guid.NewGuid().ToString());
            var list = await svc.AddListAsync("Chores");
            var t1 = await svc.AddTodoAsync(list.Id, "Clean");
            var t2 = await svc.AddTodoAsync(list.Id, "Cook");

            var deleted = await svc.DeleteTodoAsync(t1.Id);

            var todos = await svc.GetTodosAsync(list.Id);
            Assert.True(deleted);
            Assert.Single(todos);
            Assert.Equal(t2.Id, todos[0].Id);
        }

        [Fact]
        public async Task DbContext_Should_Track_TodoLists()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var list = new TodoList { Name = "DirectTest" };

            await using (var ctx = new TodoContext(options))
            {
                ctx.TodoLists.Add(list);
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = new TodoContext(options))
            {
                var result = await ctx.TodoLists.FirstOrDefaultAsync();
                Assert.NotNull(result);
                Assert.Equal("DirectTest", result!.Name);
            }
        }

        [Fact]
        public async Task DbContext_Should_Track_Todos()
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var todo = new Todo { Text = "MutationCover", TodoListId = 1 };

            await using (var ctx = new TodoContext(options))
            {
                ctx.Todos.Add(todo);
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = new TodoContext(options))
            {
                var result = await ctx.Todos.FirstOrDefaultAsync();
                Assert.NotNull(result);
                Assert.Equal("MutationCover", result!.Text);
            }
        }
    }
}
