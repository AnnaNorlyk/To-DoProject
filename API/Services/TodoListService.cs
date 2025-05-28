using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;
using FeatureHubSDK;

namespace API.Services
{
    public class TodoListService : ITodoListService
    {
        private readonly TodoContext _ctx;
        private readonly ILogger<TodoListService> _logger;
        private readonly IClientContext _fh;

        public TodoListService(TodoContext ctx, ILogger<TodoListService> logger, IClientContext fh)
        {
            _ctx = ctx;
            _logger = logger;
            _fh = fh;
        }

        public async Task<TodoListDto> AddListAsync(string name)
        {
            var list = new TodoList { Name = name, Created = DateTime.UtcNow };
            _ctx.TodoLists.Add(list);
            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Added new list '{Name}' with ID {Id}", name, list.Id);
            return new TodoListDto
            {
                Id = list.Id,
                Name = list.Name,
                Created = list.Created,
                Todos = new List<TodoDto>()
            };
        }

        public async Task<bool> DeleteListAsync(int listId)
        {
            if (_fh == null || !_fh.IsSet("enableListDeletion") || !_fh["enableListDeletion"].IsEnabled)
            {
                _logger.LogWarning("List deletion feature is disabled.");
                return false;
            }

            var list = await _ctx.TodoLists.FindAsync(listId);
            if (list == null)
            {
                _logger.LogWarning("No list was found with ID {ListId}.", listId);
                return false;
            }

            _ctx.TodoLists.Remove(list);
            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Successfully deleted list with ID {ListId}.", listId);
            return true;
        }

        public async Task<List<TodoListDto>> GetAllListsAsync()
        {
            _logger.LogInformation("Retrieving all lists from database");
            var lists = await _ctx.TodoLists
                .Include(l => l.Todos)
                .OrderBy(l => l.Id)
                .ToListAsync();

            return lists.Select(l => new TodoListDto
            {
                Id = l.Id,
                Name = l.Name,
                Created = l.Created,
                Todos = l.Todos.Select(t => new TodoDto
                {
                    Id = t.Id,
                    Text = t.Text,
                    Created = t.Created
                }).ToList()
            }).ToList();
        }

        public async Task<TodoDto> AddTodoAsync(int listId, string text)
        {
            var todo = new Todo { Text = text, Created = DateTime.UtcNow, TodoListId = listId };
            _ctx.Todos.Add(todo);
            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Added todo '{Text}' to list {ListId}", text, listId);
            return new TodoDto
            {
                Id = todo.Id,
                Text = todo.Text,
                Created = todo.Created
            };
        }

        public async Task<TodoDto?> UpdateTodoAsync(int todoId, string newText)
        {
            var todo = await _ctx.Todos.FindAsync(todoId);
            if (todo == null)
            {
                _logger.LogWarning("Todo with ID {Id} not found for update", todoId);
                return null;
            }

            todo.Text = newText;
            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Updated todo ID {Id} with new text", todoId);
            return new TodoDto
            {
                Id = todo.Id,
                Text = todo.Text,
                Created = todo.Created
            };
        }

        public async Task<bool> DeleteTodoAsync(int todoId)
        {
            var todo = await _ctx.Todos.FindAsync(todoId);
            if (todo == null)
            {
                _logger.LogWarning("Todo with ID {Id} not found for deletion", todoId);
                return false;
            }

            _ctx.Todos.Remove(todo);
            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Deleted todo with ID {Id}", todoId);
            return true;
        }

        public async Task<List<TodoDto>> GetTodosAsync(int listId)
        {
            _logger.LogInformation("Fetching todos for list ID {Id}", listId);
            return await _ctx.Todos
                .Where(t => t.TodoListId == listId)
                .OrderBy(t => t.Id)
                .Select(t => new TodoDto
                {
                    Id = t.Id,
                    Text = t.Text,
                    Created = t.Created
                })
                .ToListAsync();
        }
    }
}
