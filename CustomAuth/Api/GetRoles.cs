using Newtonsoft.Json;
using StaticWebAppAuthentication.Models;

namespace Api;

public static class GetRoles
{
    [FunctionName(nameof(GetRoles))]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "GetRoles")]
        HttpRequest req,
        ILogger log)
    {
        var content = await new StreamReader(req.Body).ReadToEndAsync();

        var clientPrincipal = JsonConvert.DeserializeObject<ClientPrincipal>(content);
        if (clientPrincipal == null)
        {
            log.LogInformation("No principal supplied");
            return new NotFoundObjectResult("No principal supplied");
        }

        var roles = new List<string>
        {
            clientPrincipal.UserDetails.Replace(" ", "_"),
            clientPrincipal.IdentityProvider,
            "authorised"
        };

        return new OkObjectResult(new
        {
            roles
        });
    }
}