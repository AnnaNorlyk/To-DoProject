using System;
using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> opts) : base(opts) { }

        public DbSet<TodoList> TodoLists { get; set; } = null!;
        public DbSet<Todo> Todos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoList>()
                .ToTable("todolists") 
                .HasMany(l => l.Todos)
                .WithOne(t => t.List!)
                .HasForeignKey(t => t.TodoListId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Todo>()
                .ToTable("todos");     
        }
    }
}
