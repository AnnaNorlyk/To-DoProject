// API.Tests/TodoContextTests.cs
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
        // Check cascade delete behavior on TodoList -> Todo relationship
        [Fact]
        public void CascadeDeleteBehavior_IsCascade()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("CascadeTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var allFks = ctx.Model
                .FindEntityType(typeof(Todo))!
                .GetForeignKeys();

            DeleteBehavior cascadeBehavior = default;
            for (int i = 0; i < allFks.Count; i++)
            {
                if (allFks[i].PrincipalEntityType.ClrType == typeof(TodoList))
                {
                    cascadeBehavior = allFks[i].DeleteBehavior;
                    break;
                }
            }

            // Assert
            Assert.Equal(DeleteBehavior.Cascade, cascadeBehavior);
        }

        // Check TodoList entity maps to "todolists" table
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

        // Check Todo entity maps to "todos" table
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

        // Check foreign key property is named "TodoListId"
        [Fact]
        public void ForeignKeyProperty_IsTodoListId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("FKTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var fkList = ctx.Model
                .FindEntityType(typeof(Todo))!
                .GetForeignKeys();

            IForeignKey matchingFk = null!;
            for (int i = 0; i < fkList.Count; i++)
            {
                if (fkList[i].PrincipalEntityType.ClrType == typeof(TodoList))
                {
                    matchingFk = fkList[i];
                    break;
                }
            }

            var fkProperty = matchingFk.Properties[0].Name;

            // Assert
            Assert.Equal("TodoListId", fkProperty);
        }

        // Check navigation properties "List" on Todo and "Todos" on TodoList exist
        [Fact]
        public void NavigationProperties_Exist()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("NavTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var todoNavs = ctx.Model
                .FindEntityType(typeof(Todo))!
                .GetNavigations()
                .Select(n => n.Name);
            var listNavs = ctx.Model
                .FindEntityType(typeof(TodoList))!
                .GetNavigations()
                .Select(n => n.Name);

            // Assert
            Assert.Contains("List", todoNavs);
            Assert.Contains("Todos", listNavs);
        }

        // Check primary key property is named "Id" on both entities
        [Fact]
        public void PrimaryKeyProperty_IsId()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("KeyTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var listKey = ctx.Model.FindEntityType(typeof(TodoList))!
                .FindPrimaryKey()!
                .Properties
                .Select(p => p.Name);
            var todoKey = ctx.Model.FindEntityType(typeof(Todo))!
                .FindPrimaryKey()!
                .Properties
                .Select(p => p.Name);

            // Assert
            Assert.Contains("Id", listKey);
            Assert.Contains("Id", todoKey);
        }

        // Check navigation metadata targets correct entity types
        [Fact]
        public void NavigationTargetTypes_AreCorrect()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TodoContext>()
                .UseInMemoryDatabase("NavTargetTest")
                .Options;

            // Act
            using var ctx = new TodoContext(options);
            var todoNav = ctx.Model.FindEntityType(typeof(Todo))!
                .FindNavigation("List")!;
            var listNav = ctx.Model.FindEntityType(typeof(TodoList))!
                .FindNavigation("Todos")!;

            // Assert
            Assert.Equal(typeof(TodoList), todoNav.TargetEntityType.ClrType);
            Assert.Equal(typeof(Todo), listNav.TargetEntityType.ClrType);
        }
    }
}
