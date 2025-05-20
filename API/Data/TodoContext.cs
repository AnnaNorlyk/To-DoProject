using Microsoft.EntityFrameworkCore;

namespace API.Data;
{

	public class TodoContext : DbContext
	{
		public TodoContext(DbContextOptions<TodoContext> opts) : base(opts) { }
		public DbSet<TodoList> TodoLists { get; set; }
		public DbSet<Todo> Todos { get; set; }
		protected override void OnModelCreating(ModelBuilder mb)
		{
			mb.Entity<TodoList>()
			.HasMany(l => l.Todos)
			.WithOne(t => t.List!)
			.HasForeignKey(t => t.TodoListId) 
			.OnDelete(DeleteBehavior.Cascade);
		}
	}
}