using Microsoft.AspNetCore.Identity;

namespace Thandizo.IdentityServer.Models
{
	public class ApplicationUser : IdentityUser
	{
		public int DefaultPassword { get; set; }
	}
}
