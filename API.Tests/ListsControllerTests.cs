using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using API.Controllers;
using API.Services;
using API.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace API.Tests
{
    public class ListsControllerLoggerTests
    {
        private readonly Mock<ITodoListService> _mockService = new();
        private readonly Mock<ILogger<ListsController>> _mockLogger = new();
        private readonly ListsController _controller;

        public ListsControllerLoggerTests()
        {
            _controller = new ListsController(_mockService.Object, _mockLogger.Object);
        }

        // Helper method to check logger message contains a given substring safely
        private static bool LoggerStateContains(object v, string text)
        {
            var state = v as object;
            return state != null && (state.ToString()?.Contains(text) ?? false);
        }


        [Fact]
        public async Task GetLists_LogsInformation()
        {
            _mockService.Setup(s => s.GetAllListsAsync()).ReturnsAsync(new List<TodoListDto>());

            var result = await _controller.GetLists();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "GET /lists - Fetching all lists")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddList_LogsWarning_OnEmptyName()
        {
            var result = await _controller.AddList("   ");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "Attempted to create list with empty name")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddList_LogsInformation_OnSuccess()
        {
            var newList = new TodoListDto { Id = 1, Name = "TestList", Created = DateTime.UtcNow, Todos = new List<TodoDto>() };
            _mockService.Setup(s => s.AddListAsync(It.IsAny<string>())).ReturnsAsync(newList);

            var result = await _controller.AddList("TestList");

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, $"Created list with ID {newList.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteList_LogsInfo_WhenSuccess()
        {
            int listId = 1;
            _mockService.Setup(s => s.DeleteListAsync(listId)).ReturnsAsync(true);

            var result = await _controller.DeleteList(listId);

            Assert.IsType<NoContentResult>(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "List deleted")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteList_LogsWarning_WhenNotFound()
        {
            int listId = 2;
            _mockService.Setup(s => s.DeleteListAsync(listId)).ReturnsAsync(false);

            var result = await _controller.DeleteList(listId);

            Assert.IsType<NotFoundResult>(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "List not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTodos_LogsInformation()
        {
            int listId = 5;
            _mockService.Setup(s => s.GetTodosAsync(listId)).ReturnsAsync(new List<TodoDto>());

            var result = await _controller.GetTodos(listId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, $"GET /lists/{listId}/todos - Fetching todos")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddTodo_LogsWarning_OnEmptyText()
        {
            int listId = 3;

            var result = await _controller.AddTodo(listId, "  ");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "Empty task text")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task AddTodo_LogsInformation_OnSuccess()
        {
            int listId = 4;
            var todoDto = new TodoDto { Id = 10, Text = "Task", Created = DateTime.UtcNow };
            _mockService.Setup(s => s.AddTodoAsync(listId, It.IsAny<string>())).ReturnsAsync(todoDto);

            var result = await _controller.AddTodo(listId, "Task");

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, $"Created todo with ID {todoDto.Id}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTodo_LogsWarning_WhenNotFound()
        {
            int todoId = 99;
            _mockService.Setup(s => s.UpdateTodoAsync(todoId, It.IsAny<string>())).ReturnsAsync((TodoDto?)null);

            var result = await _controller.UpdateTodo(todoId, "New text");

            var notFound = Assert.IsType<NotFoundResult>(result.Result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "Todo not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTodo_LogsInformation_OnSuccess()
        {
            int todoId = 20;
            var updatedDto = new TodoDto { Id = todoId, Text = "Updated", Created = DateTime.UtcNow };
            _mockService.Setup(s => s.UpdateTodoAsync(todoId, It.IsAny<string>())).ReturnsAsync(updatedDto);

            var result = await _controller.UpdateTodo(todoId, "Updated");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "Todo updated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTodo_LogsInformation_WhenSuccess()
        {
            int todoId = 5;
            _mockService.Setup(s => s.DeleteTodoAsync(todoId)).ReturnsAsync(true);

            var result = await _controller.DeleteTodo(todoId);

            Assert.IsType<NoContentResult>(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "Todo deleted")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteTodo_LogsWarning_WhenNotFound()
        {
            int todoId = 6;
            _mockService.Setup(s => s.DeleteTodoAsync(todoId)).ReturnsAsync(false);

            var result = await _controller.DeleteTodo(todoId);

            Assert.IsType<NotFoundResult>(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => LoggerStateContains(v, "Todo not found")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
