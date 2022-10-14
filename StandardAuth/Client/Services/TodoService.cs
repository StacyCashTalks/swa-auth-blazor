using Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Client.Services;

public class TodoService
{
    
    private List<Todo>? todos;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly HttpClient _httpClient;

    public IList<Todo>? Todos => todos?.AsReadOnly() ?? null;

    public TodoService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentNullException.ThrowIfNull(authenticationStateProvider, nameof(authenticationStateProvider));

        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task LoadTodos()
    {
        if (todos == null)
        {
            if (await IsAuthenticated())
            {
                todos = await _httpClient.GetFromJsonAsync<List<Todo>>("api/todos");
            }
            else
            {
                todos = new List<Todo>();
            }
        }
    }

    public async Task AddTodo(Todo todo)
    {
        if (todos is null)
        {
            return;
        }

        if (await IsAuthenticated())
        {
            await _httpClient.PostAsJsonAsync("api/todos", todo);
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

        if (await IsAuthenticated())
        {
            var putResult = await _httpClient.PutAsJsonAsync("api/todos", todo);

            putResult.EnsureSuccessStatusCode();
        }

        storedTodo.Label = todo.Label;
        storedTodo.Complete = todo.Complete;
    }

    private async Task<bool> IsAuthenticated()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        return user.HasClaim(ClaimTypes.Role, "authorised");
    }
}
