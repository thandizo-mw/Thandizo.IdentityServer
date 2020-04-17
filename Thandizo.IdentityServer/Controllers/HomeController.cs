using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Thandizo.IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ConfigurationDbContext _context;

        public HomeController(ConfigurationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetClients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _context.Clients.OrderBy(x => x.ClientName).ToListAsync();
            return Ok(clients);
        }
    }
}
