using Models;

namespace Client.Services;

public class TodoService
{
    
    private List<Todo>? todos;

    public IList<Todo>? Todos => todos?.AsReadOnly() ?? null;

    public async Task LoadTodos()
    {
        if (todos == null)
        {
            todos = new List<Todo>();
        }
    }

    public async Task AddTodo(Todo todo)
    {
        if (todos is null)
        {
            return;
        }

        todos.Add(todo);
    }

    public async Task UpdateTodo(Todo todo)
    {
        if (todos is null)
        {
            return;
        }

        var storedTodo = todos.Find(t => t.Id == todo.Id);
        if (storedTodo is null)
        {
            throw new ArgumentException($"Cannot find Todo with id {todo.Id}");
        }

        storedTodo.Label = todo.Label;
        storedTodo.Complete = todo.Complete;
    }
}
