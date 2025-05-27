using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using API.Data;
using API.Models;

namespace API.Tests
{
    public class TodoContextModelTests
    {
        // Ensures: the FK from Todo to TodoList uses cascade delete
        [Fact]
        public void Cascade()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("CascadeTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var rel = ctx.Model
                .FindEntityType(typeof(Todo))!
                .GetForeignKeys()
                .First(fk => fk.PrincipalEntityType.ClrType == typeof(TodoList));

            // Assert
            Assert.Equal(DeleteBehavior.Cascade, rel.DeleteBehavior);
        }

        // Ensures: TodoList maps to "todolists" table
        [Fact]
        public void ListTable()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("TableTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var table = ctx.Model.FindEntityType(typeof(TodoList))?.GetTableName();

            // Assert
            Assert.Equal("todolists", table);
        }

        // Ensures: Todo maps to "todos" table
        [Fact]
        public void TodoTable()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("TodoTableTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var table = ctx.Model.FindEntityType(typeof(Todo))?.GetTableName();

            // Assert
            Assert.Equal("todos", table);
        }
    }
}
