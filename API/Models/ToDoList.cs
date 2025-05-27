using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class TodoList
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        [Column("created")]
        public DateTime Created { get; set; }

        public List<Todo> Todos { get; set; } = new();
    }
}
