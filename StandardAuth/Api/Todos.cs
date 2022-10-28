using Microsoft.Azure.Cosmos;
using Models;
using StaticWebAppAuthentication.Models;
using StaticWebAppAuthentication.Api;

namespace Api
{
    public static class Todos
    {
        [FunctionName($"{nameof(Todos)}_Get")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "todos")]
            HttpRequest request,
            [CosmosDB(Connection = "TodosDbConnectionString")]
            CosmosClient client,
            ILogger log)
        {
            var containerClient = await GetCosmosContainerClient(client);
            ClientPrincipal clientPrincipal =
                StaticWebAppApiAuthorization
                    .ParseHttpHeaderForClientPrinciple(request.Headers);

            QueryDefinition query = new QueryDefinition(
                    @"select
                    t.id,
                    t.label,
                    t.complete
                  from t
                  where t.userId = @userId")
                .WithParameter("@userId", clientPrincipal.UserId);

            List<Todo> todos = new();
            var todosIterator = containerClient.GetItemQueryIterator<Todo>(query, null, new QueryRequestOptions());
            while (todosIterator.HasMoreResults)
            {
                todos.AddRange(await todosIterator.ReadNextAsync());
            }
            return new OkObjectResult(todos);
        }

        [FunctionName($"{nameof(Todos)}_Post")]
        public static async Task<IActionResult> PostTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")]
            Todo todo,
            HttpRequest request,
            [CosmosDB(Connection = "TodosDbConnectionString")]
            CosmosClient client,
            ILogger log)
        {
            if (todo.Id != default)
            {
                return new BadRequestObjectResult("id must be null");
            }

            var clientPrincipal =
                StaticWebAppApiAuthorization
                    .ParseHttpHeaderForClientPrinciple(request.Headers);

            todo.Id = Guid.NewGuid();

            var savedTodoPost = new
            {
                id = todo.Id.ToString(),
                label = todo.Label,
                complete = todo.Complete,
                userId = clientPrincipal.UserId
            };

            var containerClient = await GetCosmosContainerClient(client);
            await containerClient.CreateItemAsync<dynamic>(savedTodoPost);

            return new OkObjectResult(todo);
        }


        [FunctionName($"{nameof(Todos)}_Put")]
        public static async Task<IActionResult> PutTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos")]
            Todo todo,
            HttpRequest request,
            [CosmosDB(Connection = "TodosDbConnectionString")]
            CosmosClient client,
            ILogger log)
        {
            var clientPrincipal =
                StaticWebAppApiAuthorization
                    .ParseHttpHeaderForClientPrinciple(request.Headers);

            QueryDefinition query = new QueryDefinition(
                    @"select
                    t.id
                  from t
                  where 
                    t.userId = @userId 
                    and t.id = @id")
                .WithParameter("@userId", clientPrincipal.UserId)
                .WithParameter("id", todo.Id.ToString());

            var containerClient = await GetCosmosContainerClient(client);
            var todosIterator = containerClient.GetItemQueryIterator<Todo>(query, null, new QueryRequestOptions());

            if (!todosIterator.HasMoreResults)
            {
                return new ForbidResult();
            }

            List<PatchOperation> operations = new()
            {
                PatchOperation.Replace($"/label", todo.Label),
                PatchOperation.Replace("/complete", todo.Complete)
            };

            todo = await containerClient.PatchItemAsync<Todo>(todo.Id.ToString(), new PartitionKey(clientPrincipal.UserId), operations);

            return new OkResult();
        }

        private static async Task<Container> GetCosmosContainerClient(CosmosClient client)
        {
            var environmentVariables = Environment.GetEnvironmentVariables();

            if (!environmentVariables.Contains("TodoDBName"))
            {
                throw new ArgumentException("Database Name Not Set");
            }

            if (!environmentVariables.Contains("TodoDBContainer"))
            {
                throw new ArgumentException("Database Container String Not Set");
            }

            var databaseName = (string)environmentVariables["TodoDBName"];
            var databaseContainer = (string)environmentVariables["TodoDBContainer"];

            await client.CreateDatabaseIfNotExistsAsync(databaseName);
            await client.GetDatabase(databaseName).CreateContainerIfNotExistsAsync(databaseContainer, "/userId");

            var containerClient = client.GetContainer(databaseName, databaseContainer);
            return containerClient;
        }
    }
}
