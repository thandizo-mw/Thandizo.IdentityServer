using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Identity.DataTransfer;
using Thandizo.IdentityServer.Helpers.PasswordGenerator;
using Thandizo.IdentityServer.Models;
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

        public async Task<IActionResult> RegisterUser(UserDTO userDTO)
        {
            var response = await _userManagementService.RegisterUserAsync(userDTO);

            if (response.IsErrorOccured)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        public async Task<IActionResult> UpdatePassword(PasswordResetDTO passwordResetDTO)
        {
            var response = await _userManagementService.UpdatePasswordAsync(passwordResetDTO);

            if (response.IsErrorOccured)
            {
                return BadRequest(response.Message);
            }

            return Ok(response.Result);

        } 
    }
}
