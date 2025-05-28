using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Tests
{
    public class TodoContextTests
    {
        // Verify cascade delete from TodoList to Todo is enabled
        [Fact]
        public void CascadeDelete_FromTodoListToTodo_IsCascade()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("CascadeDeleteTest")
                .Options;

            // Act
            using var context = new TodoContext(options);
            var foreignKeys = context.Model
                .FindEntityType(typeof(Todo))!
                .GetForeignKeys()
                .ToList();

            var cascadeBehavior = foreignKeys
                .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(TodoList))
                ?.DeleteBehavior;

            // Assert
            Assert.Equal(DeleteBehavior.Cascade, cascadeBehavior);
        }

        // Verify TodoList maps to "todolists" table
        [Fact]
        public void TodoList_MapsTo_TableTodolists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("TodoListTableTest")
                .Options;

            // Act
            using var context = new TodoContext(options);
            var tableName = context.Model.FindEntityType(typeof(TodoList))?
                .GetTableName();

            // Assert
            Assert.Equal("todolists", tableName);
        }

        // Verify Todo maps to "todos" table
        [Fact]
        public void Todo_MapsTo_TableTodos()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("TodoTableTest")
                .Options;

            // Act
            using var context = new TodoContext(options);
            var tableName = context.Model.FindEntityType(typeof(Todo))?
                .GetTableName();

            // Assert
            Assert.Equal("todos", tableName);
        }

        // Verify foreign key property in Todo to TodoList is "TodoListId"
        [Fact]
        public void Todo_ForeignKeyProperty_IsTodoListId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("ForeignKeyPropertyTest")
                .Options;

            // Act
            using var context = new TodoContext(options);
            var foreignKey = context.Model
                .FindEntityType(typeof(Todo))!
                .GetForeignKeys()
                .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(TodoList));

            var propertyName = foreignKey?.Properties[0].Name;

            // Assert
            Assert.Equal("TodoListId", propertyName);
        }

        // Verify navigation properties exist on both entities
        [Fact]
        public void NavigationProperties_ExistOnEntities()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("NavigationPropertiesTest")
                .Options;

            // Act
            using var context = new TodoContext(options);
            var todoNavs = context.Model
                .FindEntityType(typeof(Todo))!
                .GetNavigations()
                .Select(n => n.Name);

            var listNavs = context.Model
                .FindEntityType(typeof(TodoList))!
                .GetNavigations()
                .Select(n => n.Name);

            // Assert
            Assert.Contains("List", todoNavs);
            Assert.Contains("Todos", listNavs);
        }

        // Verify primary key property named "Id" on TodoList and Todo
        [Fact]
        public void PrimaryKeyProperty_IsIdOnBothEntities()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("PrimaryKeyTest")
                .Options;

            // Act
            using var context = new TodoContext(options);
            var listKeyProps = context.Model.FindEntityType(typeof(TodoList))!
                .FindPrimaryKey()!
                .Properties
                .Select(p => p.Name);

            var todoKeyProps = context.Model.FindEntityType(typeof(Todo))!
                .FindPrimaryKey()!
                .Properties
                .Select(p => p.Name);

            // Assert
            Assert.Contains("Id", listKeyProps);
            Assert.Contains("Id", todoKeyProps);
        }

        // Verify navigation properties target correct entity types
        [Fact]
        public void NavigationProperties_TargetCorrectEntityTypes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("NavigationTargetTest")
                .Options;

            // Act
            using var context = new TodoContext(options);
            var todoNav = context.Model.FindEntityType(typeof(Todo))!
                .FindNavigation("List");

            var listNav = context.Model.FindEntityType(typeof(TodoList))!
                .FindNavigation("Todos");

            // Assert
            Assert.Equal(typeof(TodoList), todoNav?.TargetEntityType.ClrType);
            Assert.Equal(typeof(Todo), listNav?.TargetEntityType.ClrType);
        }
    }
}
