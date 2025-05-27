namespace API.DTOs
{
    public class TodoDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public DateTime Created { get; set; }
    }
}
