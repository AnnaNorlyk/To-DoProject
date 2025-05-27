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

        // GET /lists should return 200 and the list of lists
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

        // POST /lists should reject empty names
        [Fact]
        public async Task AddList_InvalidName_ReturnsBadRequest()
        {
            // Act
            var result = await _ctrl.AddList("");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // POST /lists should reject whitespace-only names
        [Fact]
        public async Task AddList_WhitespaceName_ReturnsBadRequest()
        {
            // Act
            var result = await _ctrl.AddList("   ");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // POST /lists should create and return the new list
        [Fact]
        public async Task AddList_ValidName_ReturnsCreated()
        {
            // Arrange
            var dto = new TodoListDto { Id = 2, Name = "Groceries" };
            _svc.Setup(s => s.AddListAsync("Groceries")).ReturnsAsync(dto);

            // Act
            var actionResult = await _ctrl.AddList("Groceries");

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(_ctrl.GetLists), created.ActionName);
            Assert.Equal(dto.Id, created.RouteValues["id"]);
            Assert.Equal(dto, created.Value);
        }

        // DELETE /lists/{id} should return 204 when deletion succeeds
        [Fact]
        public async Task DeleteList_Existing_ReturnsNoContent()
        {
            // Arrange
            _svc.Setup(s => s.DeleteListAsync(1)).ReturnsAsync(true);

            // Act & Assert
            Assert.IsType<NoContentResult>(await _ctrl.DeleteList(1));
        }

        // DELETE /lists/{id} should return 404 when not found
        [Fact]
        public async Task DeleteList_Missing_ReturnsNotFound()
        {
            // Arrange
            _svc.Setup(s => s.DeleteListAsync(9)).ReturnsAsync(false);

            // Act & Assert
            Assert.IsType<NotFoundResult>(await _ctrl.DeleteList(9));
        }

        // GET /lists/{id}/todos should return 200 and the todos
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

        // POST /lists/{id}/todos should reject empty text
        [Fact]
        public async Task AddTodo_InvalidText_ReturnsBadRequest()
        {
            // Act
            var result = await _ctrl.AddTodo(1, "");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // POST /lists/{id}/todos should reject whitespace-only text
        [Fact]
        public async Task AddTodo_WhitespaceText_ReturnsBadRequest()
        {
            // Act
            var result = await _ctrl.AddTodo(1, "   ");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // POST /lists/{id}/todos should create and return the todo
        [Fact]
        public async Task AddTodo_ValidText_ReturnsCreated()
        {
            // Arrange
            var dto = new TodoDto { Id = 3, Text = "Walk dog" };
            _svc.Setup(s => s.AddTodoAsync(1, "Walk dog")).ReturnsAsync(dto);

            // Act
            var actionResult = await _ctrl.AddTodo(1, "Walk dog");

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(_ctrl.GetTodos), created.ActionName);
            Assert.Equal(1, created.RouteValues["listId"]);
            Assert.Equal(dto, created.Value);
        }

        // PUT /api/todos/{id} should update and return todo if exists
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

        // PUT /api/todos/{id} should return 404 if todo not found
        [Fact]
        public async Task UpdateTodo_Missing_ReturnsNotFound()
        {
            // Arrange
            _svc.Setup(s => s.UpdateTodoAsync(99, "Nope")).ReturnsAsync((TodoDto?)null);

            // Act & Assert
            Assert.IsType<NotFoundResult>((await _ctrl.UpdateTodo(99, "Nope")).Result);
        }

        // DELETE /api/todos/{id} should return 204 when deletion succeeds
        [Fact]
        public async Task DeleteTodo_Existing_ReturnsNoContent()
        {
            // Arrange
            _svc.Setup(s => s.DeleteTodoAsync(10)).ReturnsAsync(true);

            // Act & Assert
            Assert.IsType<NoContentResult>(await _ctrl.DeleteTodo(10));
        }

        // DELETE /api/todos/{id} should return 404 when not found
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
