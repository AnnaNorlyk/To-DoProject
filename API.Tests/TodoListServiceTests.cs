using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using API.Data;
using API.Models;
using API.Services;
using FeatureHubSDK;

namespace API.Tests
{
    public class TodoListServiceTests
    {
        private static (TodoListService service, TodoContext ctx, Mock<ILogger<TodoListService>> logger, Mock<IClientContext> fh) CreateService(string dbName, bool flagEnabled = true)
        {
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var ctx = new TodoContext(options);

            var logger = new Mock<ILogger<TodoListService>>();
            var fh = new Mock<IClientContext>();
            fh.Setup(f => f.IsSet("enableListDeletion")).Returns(flagEnabled);
            var feature = new Mock<IFeature>();
            feature.Setup(f => f.IsEnabled).Returns(flagEnabled);
            fh.Setup(f => f["enableListDeletion"]).Returns(feature.Object);

            var service = new TodoListService(ctx, logger.Object, fh.Object);
            return (service, ctx, logger, fh);
        }

        [Fact]
        public async Task AddList_SavesAndLogs()
        {
            var (service, ctx, logger, _) = CreateService("AddListTest");

            var result = await service.AddListAsync("New List");

            var saved = await ctx.TodoLists.FindAsync(result.Id);
            Assert.NotNull(saved);
            Assert.Equal("New List", saved!.Name);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("Added new list")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteList_FlagDisabled_ReturnsFalseAndLogs()
        {
            var (service, _, logger, _) = CreateService("DeleteListFlagOff", flagEnabled: false);

            var result = await service.DeleteListAsync(1);

            Assert.False(result);
            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("List deletion feature is disabled")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteList_NotFound_ReturnsFalseAndLogs()
        {
            var (service, _, logger, _) = CreateService("DeleteListNotFound");

            var result = await service.DeleteListAsync(999);

            Assert.False(result);
            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("No list was found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteList_Success_DeletesAndLogs()
        {
            var (service, ctx, logger, _) = CreateService("DeleteListSuccess");

            var list = new TodoList { Name = "ToDelete", Created = DateTime.UtcNow };
            ctx.TodoLists.Add(list);
            await ctx.SaveChangesAsync();

            var result = await service.DeleteListAsync(list.Id);

            Assert.True(result);
            var found = await ctx.TodoLists.FindAsync(list.Id);
            Assert.Null(found);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("Successfully deleted")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddTodo_CreatesAndLogs()
        {
            var (service, ctx, logger, _) = CreateService("AddTodoTest");

            var list = new TodoList { Name = "List", Created = DateTime.UtcNow };
            ctx.TodoLists.Add(list);
            await ctx.SaveChangesAsync();

            var todo = await service.AddTodoAsync(list.Id, "Task");

            var found = await ctx.Todos.FindAsync(todo.Id);
            Assert.NotNull(found);
            Assert.Equal("Task", found!.Text);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("Added todo")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTodo_UpdatesAndLogs()
        {
            var (service, ctx, logger, _) = CreateService("UpdateTodoTest");

            var list = new TodoList { Name = "List", Created = DateTime.UtcNow };
            ctx.TodoLists.Add(list);
            await ctx.SaveChangesAsync();

            var todo = new Todo { Text = "Old", Created = DateTime.UtcNow, TodoListId = list.Id };
            ctx.Todos.Add(todo);
            await ctx.SaveChangesAsync();

            var updated = await service.UpdateTodoAsync(todo.Id, "New");

            Assert.NotNull(updated);
            Assert.Equal("New", updated!.Text);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("Updated todo")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTodo_ReturnsNullAndLogsWarning()
        {
            var (service, _, logger, _) = CreateService("UpdateTodoNotFound");

            var updated = await service.UpdateTodoAsync(999, "New");

            Assert.Null(updated);

            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("not found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTodo_DeletesAndLogs()
        {
            var (service, ctx, logger, _) = CreateService("DeleteTodoTest");

            var list = new TodoList { Name = "List", Created = DateTime.UtcNow };
            ctx.TodoLists.Add(list);
            await ctx.SaveChangesAsync();

            var todo = new Todo { Text = "Task", Created = DateTime.UtcNow, TodoListId = list.Id };
            ctx.Todos.Add(todo);
            await ctx.SaveChangesAsync();

            var deleted = await service.DeleteTodoAsync(todo.Id);

            Assert.True(deleted);

            var found = await ctx.Todos.FindAsync(todo.Id);
            Assert.Null(found);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("Deleted todo")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTodo_ReturnsFalseAndLogsWarning()
        {
            var (service, _, logger, _) = CreateService("DeleteTodoNotFound");

            var deleted = await service.DeleteTodoAsync(999);

            Assert.False(deleted);

            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("not found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTodos_ReturnsTodosAndLogs()
        {
            var (service, ctx, logger, _) = CreateService("GetTodosTest");

            var list = new TodoList { Name = "List", Created = DateTime.UtcNow };
            ctx.TodoLists.Add(list);
            await ctx.SaveChangesAsync();

            ctx.Todos.Add(new Todo { Text = "Task1", Created = DateTime.UtcNow, TodoListId = list.Id });
            ctx.Todos.Add(new Todo { Text = "Task2", Created = DateTime.UtcNow, TodoListId = list.Id });
            await ctx.SaveChangesAsync();

            var todos = await service.GetTodosAsync(list.Id);

            Assert.Equal(2, todos.Count);

            logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<object>(v => v != null && v.ToString() != null && v.ToString()!.Contains("Fetching todos")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }
    }
}
