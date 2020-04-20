using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Thandizo.IdentityServer.Models;

namespace Thandizo.IdentityServer.Data
{
    public static class DataSeeder
    {
        public static async Task UsersSeed(UserManager<ApplicationUser> userManager)
        {
            var password = "Supreme";

            var alice = new ApplicationUser
            {
                Id = "1",
                UserName = "+265999201886",
                NormalizedUserName = "+265999201886"
            };
            alice.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(alice, password);

            var bob = new ApplicationUser
            {
                Id = "2",
                UserName = "+265888952783",
                NormalizedUserName = "+265888952783",
            };
            bob.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(bob, password);

            await userManager.CreateAsync(alice);
            await userManager.CreateAsync(bob);

            var user1 = await userManager.FindByNameAsync(alice.UserName);
            
            IList<Claim> user1ClaimList = new List<Claim>();
            user1ClaimList.Add(new Claim("name", alice.UserName));
            await userManager.AddClaimsAsync(user1, user1ClaimList);


            var user2 = await userManager.FindByNameAsync(bob.UserName);
            IList<Claim> user2ClaimList = new List<Claim>();
            user2ClaimList.Add(new Claim("name", bob.UserName));
            await userManager.AddClaimsAsync(user2, user2ClaimList);

        }

    }
}
