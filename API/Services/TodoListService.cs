using API.Data;
using API.DTOs;
using API.Models;
using API.Services;
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
                _logger.LogWarning("List deletion disabled by feature toggle.");
                return false;
            }

            var list = await _ctx.TodoLists.FindAsync(listId);
            if (list is null)
            {
                _logger.LogWarning("No list found with ID {ListId}.", listId);
                return false;
            }

            _ctx.TodoLists.Remove(list);
            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Deleted list with ID {ListId}.", listId);
            return true;
        }

        public async Task<List<TodoListDto>> GetAllListsAsync()
        {
            _logger.LogInformation("Retrieving all lists");
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
            var todo = new Todo
            {
