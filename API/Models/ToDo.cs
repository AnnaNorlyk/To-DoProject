public class Todo
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public DateTime Created { get; set; }
    public int TodoListId { get; set; }
    public TodoList? List { get; set; }
}