using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;


namespace API.Services
{
    public interface ITodoListService
    {
        // Lists
        Task<TodoListDto> AddListAsync(string name);
        Task<bool> DeleteListAsync(int listId);
        Task<List<TodoListDto>> GetAllListsAsync();

        // Todos within a list
        Task<TodoDto> AddTodoAsync(int listId, string text);
        Task<TodoDto?> UpdateTodoAsync(int todoId, string newText);
        Task<bool> DeleteTodoAsync(int todoId);
        Task<List<TodoDto>> GetTodosAsync(int listId);
    }
}
