using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Thandizo.IdentityServer.Data;
using Thandizo.IdentityServer.Models;

namespace Thandizo.IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ConfigurationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ConfigurationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("GetClients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _context.Clients.OrderBy(x => x.ClientName).ToListAsync();
            return Ok(clients);
        }

        [HttpGet("SeedUsers")]
        public async Task<IActionResult> SeedUsers()
        {
            await DataSeeder.UsersSeed(_userManager);
            return Ok();
        }

    }
}
