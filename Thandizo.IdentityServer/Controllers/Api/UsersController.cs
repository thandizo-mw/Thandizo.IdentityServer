using Microsoft.AspNetCore.Mvc;
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

    }
}
