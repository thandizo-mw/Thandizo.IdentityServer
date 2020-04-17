using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Thandizo.IdentityServer.Models;

namespace Thandizo.IdentityServer.Data.Migrations.AspNetIdentity
{
    public class ThandizoIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public ThandizoIdentityDbContext(DbContextOptions<ThandizoIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
