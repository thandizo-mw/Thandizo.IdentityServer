using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Identity.DataTransfer;
using Thandizo.IdentityServer.Helpers.PasswordGenerator;
using Thandizo.IdentityServer.Models;


namespace Thandizo.IdentityServer.Services
{

    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        SignInManager<ApplicationUser> _signInManager;

        public UserManagementService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<OutputResponse> RegisterUserAsync(UserDTO userDTO)
        {
            var password = "";
            var response = new OutputResponse { IsErrorOccured = true };
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    RandomPasswordGenerator randomPasswordGenerator = new RandomPasswordGenerator();

                    password = randomPasswordGenerator.GeneratePassword(true, true, true, true, 16);

                    var newUser = new ApplicationUser
                    {
                        UserName = userDTO.PhoneNumber,
                        NormalizedUserName = userDTO.PhoneNumber,
                        DefaultPassword = 0
                    };
                    newUser.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(newUser, password);

                    await _userManager.CreateAsync(newUser);

                    var user = await _userManager.FindByNameAsync(newUser.UserName);

                    IList<Claim> userClaimList = new List<Claim>();
                    userClaimList.Add(new Claim("name", user.UserName));
                    await _userManager.AddClaimsAsync(user, userClaimList);

                    response = new OutputResponse
                    {
                        IsErrorOccured = false,
                        Result = password
                    };
                    scope.Complete();
                }
            }
            catch (TransactionAbortedException)
            {
                response = new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "An error occured while trying to create your user account, try again"
                };

            }
            catch (ApplicationException)
            {
                response = new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "An error occured while trying to create your user account, try again"
                };

            }

            return response;
        }

        public async Task<OutputResponse> UpdatePasswordAsync(PasswordResetDTO passwordResetDTO)
        {
            var userName = passwordResetDTO.Username;
            var user = await _userManager.FindByNameAsync(userName);
            var response = new OutputResponse { IsErrorOccured = true };

            if (user == null)
            {
                response = new OutputResponse() { IsErrorOccured = true, Message = "Thandizo couldn't find your user details, please try again later" };
            }

            var isOldPassword = await _userManager.CheckPasswordAsync(user, passwordResetDTO.NewPassword);

            if (isOldPassword)
            {
                response = new OutputResponse() { IsErrorOccured = true, Message = "Cannot use an old password, please try a different password" };
            }

            if (!passwordResetDTO.NewPassword.Equals(passwordResetDTO.NewPasswordConfirmation))
            {
                response = new OutputResponse() { IsErrorOccured = true, Message = "Your confirmation password does not match your new password" };
            }

            var passwordReset = await _userManager.ChangePasswordAsync(user, passwordResetDTO.CurrentPassword, passwordResetDTO.NewPassword);
            if (!passwordReset.Succeeded)
            {
                response = new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Sorry Thandizo failed to update your password, please try again"
                };
            }

            response = new OutputResponse
            {
                IsErrorOccured = false,
                Result = passwordResetDTO.NewPassword
            };

            return response;
        }
    }
}
