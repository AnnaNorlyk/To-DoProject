using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Controllers;
using API.DTOs;
using API.Services;

namespace API.Tests
{
    public class ListsControllerTests
    {
        private readonly Mock<ITodoListService> _mockService = new();
        private readonly Mock<ILogger<ListsController>> _mockLogger = new();
        private readonly ListsController _controller;

        public ListsControllerTests()
        {
            _controller = new ListsController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLists_ReturnsOkWithLists()
        {
            var lists = new List<TodoListDto> { new() { Id = 1, Name = "Test" } };
            _mockService.Setup(s => s.GetAllListsAsync()).ReturnsAsync(lists);

            var result = await _controller.GetLists();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(lists, ok.Value);
        }

        [Fact]
        public async Task AddList_ReturnsBadRequest_IfNameIsEmpty()
        {
            var result = await _controller.AddList("");

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddList_ReturnsCreatedList()
        {
            var list = new TodoListDto { Id = 1, Name = "Groceries" };
            _mockService.Setup(s => s.AddListAsync("Groceries")).ReturnsAsync(list);

            var result = await _controller.AddList("Groceries");

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(list, created.Value);
        }

        [Fact]
        public async Task DeleteList_ReturnsNoContent_IfSuccessful()
        {
            _mockService.Setup(s => s.DeleteListAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteList(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteList_ReturnsNotFound_IfNotFound()
        {
            _mockService.Setup(s => s.DeleteListAsync(2)).ReturnsAsync(false);

            var result = await _controller.DeleteList(2);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetTodos_ReturnsOkWithTodos()
        {
            var todos = new List<TodoDto> { new() { Id = 1, Text = "Buy milk" } };
            _mockService.Setup(s => s.GetTodosAsync(1)).ReturnsAsync(todos);

            var result = await _controller.GetTodos(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(todos, ok.Value);
        }

        [Fact]
        public async Task AddTodo_ReturnsBadRequest_IfEmpty()
        {
            var result = await _controller.AddTodo(1, "");

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task AddTodo_ReturnsCreatedTodo()
        {
            var todo = new TodoDto { Id = 1, Text = "Walk dog" };
            _mockService.Setup(s => s.AddTodoAsync(1, "Walk dog")).ReturnsAsync(todo);

            var result = await _controller.AddTodo(1, "Walk dog");

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(todo, created.Value);
        }

        [Fact]
        public async Task UpdateTodo_ReturnsOk_IfFound()
        {
            var updated = new TodoDto { Id = 1, Text = "Updated" };
            _mockService.Setup(s => s.UpdateTodoAsync(1, "Updated")).ReturnsAsync(updated);

            var result = await _controller.UpdateTodo(1, "Updated");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(updated, ok.Value);
        }

        [Fact]
        public async Task UpdateTodo_ReturnsNotFound_IfNull()
        {
            _mockService.Setup(s => s.UpdateTodoAsync(99, "Updated")).ReturnsAsync((TodoDto?)null);

            var result = await _controller.UpdateTodo(99, "Updated");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteTodo_ReturnsNoContent_IfSuccessful()
        {
            _mockService.Setup(s => s.DeleteTodoAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteTodo(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTodo_ReturnsNotFound_IfNotFound()
        {
            _mockService.Setup(s => s.DeleteTodoAsync(2)).ReturnsAsync(false);

            var result = await _controller.DeleteTodo(2);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
