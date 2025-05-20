using Microsoft.EntityFrameworkCore;

public class TodoListService : ITodoListService
{
    private readonly TodoContext _ctx;
    public TodoListService(TodoContext ctx) => _ctx = ctx;

    public async Task<TodoList> AddListAsync(string name)
    {
        var list = new TodoList { Name = name, Created = DateTime.UtcNow };
        _ctx.TodoLists.Add(list);
        await _ctx.SaveChangesAsync();
        return list;
    }

    public async Task<bool> DeleteListAsync(int listId)
    {
        var list = await _ctx.TodoLists.FindAsync(listId);
        if (list is null) return false;
        _ctx.TodoLists.Remove(list);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public Task<List<TodoList>> GetAllListsAsync() =>
        _ctx.TodoLists.Include(l => l.Todos)
                      .OrderBy(l => l.Id)
                      .ToListAsync();

    public async Task<Todo> AddTodoAsync(int listId, string text)
    {
        var todo = new Todo { Text = text, Created = DateTime.UtcNow, TodoListId = listId };
        _ctx.Todos.Add(todo);
        await _ctx.SaveChangesAsync();
        return todo;
    }

    public async Task<Todo?> UpdateTodoAsync(int todoId, string newText)
    {
        var todo = await _ctx.Todos.FindAsync(todoId);
        if (todo is null) return null;
        todo.Text = newText;
        await _ctx.SaveChangesAsync();
        return todo;
    }

    public async Task<bool> DeleteTodoAsync(int todoId)
    {
        var todo = await _ctx.Todos.FindAsync(todoId);
        if (todo is null) return false;
        _ctx.Todos.Remove(todo);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public Task<List<Todo>> GetTodosAsync(int listId) =>
        _ctx.Todos.Where(t => t.TodoListId == listId)
                  .OrderBy(t => t.Id)
                  .ToListAsync();
}
