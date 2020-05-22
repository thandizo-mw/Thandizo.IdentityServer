using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Thandizo.DataModels.Identity.DataTransfer;
using Thandizo.IdentityServer.Services;

namespace Thandizo.IdentityServer.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserManagementService _userManagementService;

        public UsersController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser(UserDTO userDTO)
        {
            var response = await _userManagementService.RegisterUserAsync(userDTO);

            if (response.IsErrorOccured)
            {
                return BadRequest(response.Message);
            }

            return Created("", response);
        }

        [HttpPost("RequestAccessToken")]
        public async Task<IActionResult> RequestAccessToken([FromBody] ClientCredentialsTokenRequest tokenRequest, [FromServices] IConfiguration configuration)
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(configuration["Authority"]);

            // discover endpoints from metadata
            if (disco.IsError)
            {
                return BadRequest("Failed to reach the Thandizo security server");
            }

            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = tokenRequest.ClientId,
                ClientSecret = tokenRequest.ClientSecret,
                Scope = tokenRequest.Scope
            });

            var token = response.AccessToken;

            if (response.IsError)
            {
                return BadRequest(response.Error);
            }

            return Ok(token);
        }

        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(UserDTO userDTO)
        {
            var response = await _userManagementService.DeleteUserAsync(userDTO);

            if (response.IsErrorOccured)
            {
                return BadRequest(response.Message);
            }

            return Created("", response);
        }

    }
}
