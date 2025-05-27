using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using API.Data;
using API.Models;

namespace API.Tests
{
    public class TodoContextModelTests
    {
        // check cascade delete behavior on TodoList -> Todo relationship
        [Fact]
        public void CascadeDeleteBehavior_IsCascade()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("CascadeTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var cascadeBehavior = ctx.Model
                .FindEntityType(typeof(Todo))!
                .GetForeignKeys()
                .First(fk => fk.PrincipalEntityType.ClrType == typeof(TodoList))
                .DeleteBehavior;

            // Assert
            Assert.Equal(DeleteBehavior.Cascade, cascadeBehavior);
        }

        // check TodoList entity maps to "todolists" table
        [Fact]
        public void TodoList_TableName_IsTodolists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("TableTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var tableName = ctx.Model.FindEntityType(typeof(TodoList))?
                .GetTableName();

            // Assert
            Assert.Equal("todolists", tableName);
        }

        // check Todo entity maps to "todos" table
        [Fact]
        public void Todo_TableName_IsTodos()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("TodoTableTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var tableName = ctx.Model.FindEntityType(typeof(Todo))?
                .GetTableName();

            // Assert
            Assert.Equal("todos", tableName);
        }

        // check foreign key property is named "TodoListId"
        [Fact]
        public void ForeignKeyProperty_IsTodoListId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("FKTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var fkProperty = ctx.Model
                .FindEntityType(typeof(Todo))!
                .GetForeignKeys()
                .First(fk => fk.PrincipalEntityType.ClrType == typeof(TodoList))
                .Properties
                .First()
                .Name;

            // Assert
            Assert.Equal("TodoListId", fkProperty);
        }

        // check navigation properties "List" on Todo and "Todos" on TodoList exist
        [Fact]
        public void NavigationProperties_Exist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("NavTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var navNames = ctx.Model
                .FindEntityType(typeof(Todo))!
                .GetNavigations()
                .Select(n => n.Name);

            // Assert
            Assert.Contains("List", navNames);
            Assert.Contains("Todos", navNames);
        }
    }
}