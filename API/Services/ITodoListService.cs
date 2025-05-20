using System.Collections.Generic;
using System.Threading.Tasks;
using API.Models;

namespace API.Services
{
    public interface ITodoListService
    {
        // Lists
        Task<TodoList> AddListAsync(string name);
        Task<bool> DeleteListAsync(int listId);
        Task<List<TodoList>> GetAllListsAsync();

        // Todos within a list
        Task<Todo> AddTodoAsync(int listId, string text);
        Task<Todo?> UpdateTodoAsync(int todoId, string newText);
        Task<bool> DeleteTodoAsync(int todoId);
        Task<List<Todo>> GetTodosAsync(int listId);
    }
}
