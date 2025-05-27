using API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Todo
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    [Column("created")] 
    public DateTime Created { get; set; }
    [Column("todo_list_id")]
    public int TodoListId { get; set; }
    public TodoList? List { get; set; }
}
