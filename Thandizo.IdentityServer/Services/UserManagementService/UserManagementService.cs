using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Identity.DataTransfer;
using Thandizo.IdentityServer.Helpers.Security;
using Thandizo.IdentityServer.Models;
using Thandizo.IdentityServer.Services.Messaging;

namespace Thandizo.IdentityServer.Services
{

    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        SignInManager<ApplicationUser> _signInManager;
        private readonly ISMSService _smsService;

        public UserManagementService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ISMSService smsService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _smsService = smsService;
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

                    password = randomPasswordGenerator.GeneratePassword(true, true, true, true, 8);

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
                    userClaimList.Add(new Claim("full_name", userDTO.FullName));
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

            if(!response.IsErrorOccured)
               await _smsService.SendSmsAsync(userDTO.PhoneNumber, password);

            return response;
        }

        public async Task<OutputResponse> UpdatePasswordAsync(PasswordChangeDTO passwordResetDTO)
        {
            var userName = passwordResetDTO.Username;
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return new OutputResponse() { IsErrorOccured = true, Message = "Thandizo couldn't find your user details, please try again later" };
            }

            var isOldPassword = await _userManager.CheckPasswordAsync(user, passwordResetDTO.NewPassword);

            if (isOldPassword)
            {
               return new OutputResponse() { IsErrorOccured = true, Message = "Cannot use an old password, please try a different password" };
            }

            if (!passwordResetDTO.NewPassword.Equals(passwordResetDTO.NewPasswordConfirmation))
            {
                return new OutputResponse() { IsErrorOccured = true, Message = "Your confirmation password does not match your new password" };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordReset = await _userManager.ResetPasswordAsync(user, token, passwordResetDTO.NewPassword);
            if (!passwordReset.Succeeded)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = passwordReset.Errors.FirstOrDefault().Description
                };
            }

            user.DefaultPassword = 1;
            await _userManager.UpdateAsync(user);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = passwordResetDTO.NewPassword
            };

        }
    }
}
