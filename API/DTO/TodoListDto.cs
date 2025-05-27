namespace API.DTOs
{
    public class TodoListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }

        public List<TodoDto> Todos { get; set; } = new();
    }
}
