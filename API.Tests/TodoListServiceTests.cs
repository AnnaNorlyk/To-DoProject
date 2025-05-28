using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Services;
using FeatureHubSDK;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace API.Tests
{
    internal sealed class TestLogger<T> : ILogger<T>
    {
        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }

        public readonly List<(LogLevel Level, string Msg)> Records = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel level) => true;

        public void Log<TState>(LogLevel level, EventId id, TState state,
                                Exception? exception,
                                Func<TState, Exception?, string> formatter)
            => Records.Add((level, formatter(state, exception)));
    }

    public sealed class TodoListServiceTests
    {
        private static (TodoListService svc, TestLogger<TodoListService> log) BuildService(bool flagEnabled = true)
        {
            var opts = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var ctx = new TodoContext(opts);

            var log = new TestLogger<TodoListService>();

            var ctxMock = new Mock<IClientContext>();
            ctxMock.Setup(c => c.IsSet("enableListDeletion")).Returns(true);
            var feature = new Mock<IFeature>();
            feature.SetupGet(f => f.IsEnabled).Returns(flagEnabled);
            ctxMock.Setup(c => c["enableListDeletion"]).Returns(feature.Object);

            return (new TodoListService(ctx, log, ctxMock.Object), log);
        }

        [Fact]
        public async Task AddList_SavesAndLogs()
        {
            var (svc, log) = BuildService();
            var dto = await svc.AddListAsync("New List");

            Assert.Equal("New List", dto.Name);
            Assert.Contains(log.Records, r => r.Msg.Contains("Added new list"));
        }

        [Fact]
        public async Task GetAllLists_Empty_Logs()
        {
            var (svc, log) = BuildService();
            var all = await svc.GetAllListsAsync();

            Assert.Empty(all);
            Assert.Contains(log.Records, r => r.Msg.Contains("Retrieving all lists"));
        }

        [Fact]
        public async Task AddTodo_SavesAndLogs()
        {
            var (svc, log) = BuildService();
            var list = await svc.AddListAsync("L");
            var todo = await svc.AddTodoAsync(list.Id, "task");

            Assert.Equal("task", todo.Text);
            Assert.Contains(log.Records, r => r.Msg.Contains("Added todo"));
        }

        [Fact]
        public async Task GetTodos_ReturnsOrdered_AndLogs()
        {
            var (svc, log) = BuildService();
            var list = await svc.AddListAsync("L");
            await svc.AddTodoAsync(list.Id, "a");
            await svc.AddTodoAsync(list.Id, "b");

            var todos = await svc.GetTodosAsync(list.Id);

            Assert.Equal(2, todos.Count);
            Assert.True(todos[0].Id < todos[1].Id);
            Assert.Contains(log.Records, r => r.Msg.Contains($"Fetching todos for list ID {list.Id}"));
        }

        [Fact]
        public async Task UpdateTodo_Existing_LogsInfo()
        {
            var (svc, log) = BuildService();
            var list = await svc.AddListAsync("L");
            var t = await svc.AddTodoAsync(list.Id, "old");

            var upd = await svc.UpdateTodoAsync(t.Id, "new");

            Assert.NotNull(upd);
            Assert.Equal("new", upd!.Text);
            Assert.Contains(log.Records, r => r.Msg.Contains("Updated todo"));
        }

        [Fact]
        public async Task UpdateTodo_NotFound_LogsWarning()
        {
            var (svc, log) = BuildService();
            var upd = await svc.UpdateTodoAsync(999, "x");

            Assert.Null(upd);
            Assert.Contains(log.Records, r => r.Level == LogLevel.Warning && r.Msg.Contains("not found"));
        }

        [Fact]
        public async Task DeleteTodo_Existing_LogsInfo()
        {
            var (svc, log) = BuildService();
            var list = await svc.AddListAsync("L");
            var t = await svc.AddTodoAsync(list.Id, "d");

            var ok = await svc.DeleteTodoAsync(t.Id);

            Assert.True(ok);
            Assert.Contains(log.Records, r => r.Msg.Contains("Deleted todo"));
        }

        [Fact]
        public async Task DeleteTodo_NotFound_LogsWarning()
        {
            var (svc, log) = BuildService();
            var ok = await svc.DeleteTodoAsync(999);

            Assert.False(ok);
            Assert.Contains(log.Records, r => r.Level == LogLevel.Warning && r.Msg.Contains("not found"));
        }

        [Fact]
        public async Task DeleteList_FlagDisabled_LogsWarning()
        {
            var (svc, log) = BuildService(flagEnabled: false);
            var list = await svc.AddListAsync("L");

            var ok = await svc.DeleteListAsync(list.Id);

            Assert.False(ok);
            Assert.Contains(log.Records, r => r.Level == LogLevel.Warning && r.Msg.Contains("feature is disabled"));
        }

        [Fact]
        public async Task DeleteList_NotFound_LogsWarning()
        {
            var (svc, log) = BuildService();
            var ok = await svc.DeleteListAsync(999);

            Assert.False(ok);
            Assert.Contains(log.Records, r => r.Level == LogLevel.Warning && r.Msg.Contains("No list was found"));
        }

        [Fact]
        public async Task DeleteList_HappyPath_LogsInfo()
        {
            var (svc, log) = BuildService();
            var list = await svc.AddListAsync("Z");

            var ok = await svc.DeleteListAsync(list.Id);

            Assert.True(ok);
            Assert.Contains(log.Records, r => r.Msg.Contains("Successfully deleted"));
        }
    }
}
