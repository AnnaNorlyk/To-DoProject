using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Controllers;
using API.DTOs;
using API.Services;

namespace API.Tests
{
    public class ListsControllerTests
    {
        private readonly Mock<ITodoListService> _svc = new();
        private readonly Mock<ILogger<ListsController>> _log = new();
        private readonly ListsController _ctrl;

        public ListsControllerTests()
        {
            _ctrl = new ListsController(_svc.Object, _log.Object);
        }

        // GetLists returns Ok
        [Fact]
        public async Task GetLists_ReturnsOk()
        {
            // Arrange
            var data = new List<TodoListDto> { new() { Id = 1, Name = "X" } };
            _svc.Setup(s => s.GetAllListsAsync()).ReturnsAsync(data);

            // Act
            var actionResult = await _ctrl.GetLists();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Same(data, ok.Value);
        }

        // AddList null returns bad request
        [Fact]
        public async Task AddList_Null_ReturnsBadRequest()
        {
            // Act
            var result = await _ctrl.AddList(null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // AddList whitespace returns bad request
        [Fact]
        public async Task AddList_Whitespace_ReturnsBadRequest()
        {
            // Act
            var result = await _ctrl.AddList("   ");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // AddList valid returns created
        [Fact]
        public async Task AddList_Valid_ReturnsCreated()
        {
            // Arrange
            var dto = new TodoListDto { Id = 2, Name = "Groceries" };
            _svc.Setup(s => s.AddListAsync("Groceries")).ReturnsAsync(dto);

            // Act
            var actionResult = await _ctrl.AddList("Groceries");

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(_ctrl.GetLists), created.ActionName);
            Assert.Equal(dto.Id, (int)created.RouteValues!["id"]!);
            Assert.Equal(dto, created.Value);
        }

        // DeleteList existing returns NoContent
        [Fact]
        public async Task DeleteList_Existing_ReturnsNoContent()
        {
            // Arrange
            _svc.Setup(s => s.DeleteListAsync(1)).ReturnsAsync(true);

            // Act & Assert
            Assert.IsType<NoContentResult>(await _ctrl.DeleteList(1));
        }

        // DeleteList missing returns NotFound
        [Fact]
        public async Task DeleteList_Missing_ReturnsNotFound()
        {
            // Arrange
            _svc.Setup(s => s.DeleteListAsync(9)).ReturnsAsync(false);

            // Act & Assert
            Assert.IsType<NotFoundResult>(await _ctrl.DeleteList(9));
        }

        // GetTodos returns Ok
        [Fact]
        public async Task GetTodos_ReturnsOk()
        {
            // Arrange
            var todos = new List<TodoDto> { new() { Id = 5, Text = "Buy milk" } };
            _svc.Setup(s => s.GetTodosAsync(1)).ReturnsAsync(todos);

            // Act
            var actionResult = await _ctrl.GetTodos(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Same(todos, ok.Value);
        }

        // GetTodos null returns Ok
        [Fact]
        public async Task GetTodos_Null_ReturnsOk()
        {
            // Arrange
            _svc.Setup(s => s.GetTodosAsync(1)).ReturnsAsync((List<TodoDto>)null);

            // Act
            var actionResult = await _ctrl.GetTodos(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Null(ok.Value);
        }

        // AddTodo null returns bad request
        [Fact]
        public async Task AddTodo_Null_ReturnsBadRequest()
        {
            // Act
            var result = await _ctrl.AddTodo(1, null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // AddTodo whitespace returns bad request
        [Fact]
        public async Task AddTodo_Whitespace_ReturnsBadRequest()
        {
            // Act
            var result = await _ctrl.AddTodo(1, "   ");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // AddTodo valid returns created
        [Fact]
        public async Task AddTodo_Valid_ReturnsCreated()
        {
            // Arrange
            var dto = new TodoDto { Id = 3, Text = "Walk dog" };
            _svc.Setup(s => s.AddTodoAsync(1, "Walk dog")).ReturnsAsync(dto);

            // Act
            var actionResult = await _ctrl.AddTodo(1, "Walk dog");

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(_ctrl.GetTodos), created.ActionName);
            Assert.Equal(1, (int)created.RouteValues!["listId"]!);
            Assert.Equal(dto, created.Value);
        }

        // UpdateTodo existing returns Ok
        [Fact]
        public async Task UpdateTodo_Existing_ReturnsOk()
        {
            // Arrange
            var dto = new TodoDto { Id = 4, Text = "Updated" };
            _svc.Setup(s => s.UpdateTodoAsync(4, "Updated")).ReturnsAsync(dto);

            // Act
            var actionResult = await _ctrl.UpdateTodo(4, "Updated");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(dto, ok.Value);
        }

        // UpdateTodo null returns NotFound
        [Fact]
        public async Task UpdateTodo_NullText_ReturnsNotFound()
        {
            // Arrange
            _svc.Setup(s => s.UpdateTodoAsync(5, null!)).ReturnsAsync((TodoDto)null);

            // Act
            var result = await _ctrl.UpdateTodo(5, null!);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        // DeleteTodo existing returns NoContent
        [Fact]
        public async Task DeleteTodo_Existing_ReturnsNoContent()
        {
            // Arrange
            _svc.Setup(s => s.DeleteTodoAsync(10)).ReturnsAsync(true);

            // Act & Assert
            Assert.IsType<NoContentResult>(await _ctrl.DeleteTodo(10));
        }

        // DeleteTodo missing returns NotFound
        [Fact]
        public async Task DeleteTodo_Missing_ReturnsNotFound()
        {
            // Arrange
            _svc.Setup(s => s.DeleteTodoAsync(20)).ReturnsAsync(false);

            // Act & Assert
            Assert.IsType<NotFoundResult>(await _ctrl.DeleteTodo(20));
        }
    }
}
