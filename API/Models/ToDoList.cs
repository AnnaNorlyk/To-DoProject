using System;
using System.Collections.Generic;

namespace API.Models;

public class TodoList
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Created { get; set; }
    public List<Todo> Todos { get; set; } = new();
}
