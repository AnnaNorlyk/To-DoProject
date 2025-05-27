using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListsController : ControllerBase
    {
        private readonly ITodoListService _service;
        private readonly ILogger<ListsController> _logger;

        public ListsController(ITodoListService service, ILogger<ListsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<TodoListDto>>> GetLists()
        {
            _logger.LogInformation("GET /lists - Fetching all lists");
            var lists = await _service.GetAllListsAsync();
            return Ok(lists);
        }

        [HttpPost]
        public async Task<ActionResult<TodoListDto>> AddList([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("POST /lists - Attempted to create list with empty name");
                return BadRequest("Name cannot be empty.");
            }

            var newList = await _service.AddListAsync(name);
            _logger.LogInformation("POST /lists - Created list with ID {Id}", newList.Id);
            return CreatedAtAction(nameof(GetLists), new { id = newList.Id }, newList);
        }

        [HttpDelete("{listId}")]
        public async Task<IActionResult> DeleteList(int listId)
        {
            var success = await _service.DeleteListAsync(listId);
            if (success)
            {
                _logger.LogInformation("DELETE /lists/{ListId} - List deleted", listId);
                return NoContent();
            }

            _logger.LogWarning("DELETE /lists/{ListId} - List not found", listId);
            return NotFound();
        }

        [HttpGet("{listId}/todos")]
        public async Task<ActionResult<List<TodoDto>>> GetTodos(int listId)
        {
            _logger.LogInformation("GET /lists/{ListId}/todos - Fetching todos", listId);
            var todos = await _service.GetTodosAsync(listId);
            return Ok(todos);
        }

        [HttpPost("{listId}/todos")]
        public async Task<ActionResult<TodoDto>> AddTodo(int listId, [FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("POST /lists/{ListId}/todos - Empty task text", listId);
                return BadRequest("Text cannot be empty.");
            }

            var todo = await _service.AddTodoAsync(listId, text);
            _logger.LogInformation("POST /lists/{ListId}/todos - Created todo with ID {Id}", listId, todo.Id);
            return CreatedAtAction(nameof(GetTodos), new { listId = listId }, todo);
        }

        [HttpPut("/api/todos/{todoId}")]
        public async Task<ActionResult<TodoDto>> UpdateTodo(int todoId, [FromBody] string newText)
        {
            var updated = await _service.UpdateTodoAsync(todoId, newText);
            if (updated is null)
            {
                _logger.LogWarning("PUT /todos/{TodoId} - Todo not found", todoId);
                return NotFound();
            }

            _logger.LogInformation("PUT /todos/{TodoId} - Todo updated", todoId);
            return Ok(updated);
        }

        [HttpDelete("/api/todos/{todoId}")]
        public async Task<IActionResult> DeleteTodo(int todoId)
        {
            var success = await _service.DeleteTodoAsync(todoId);
            if (success)
            {
                _logger.LogInformation("DELETE /todos/{TodoId} - Todo deleted", todoId);
                return NoContent();
            }

            _logger.LogWarning("DELETE /todos/{TodoId} - Todo not found", todoId);
            return NotFound();
        }
    }
}
