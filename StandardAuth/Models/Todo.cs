namespace Models;

public class Todo
{
    public Guid Id { get; set; }
    public string? Label { get; set; }
    public bool Complete { get; set; }
}