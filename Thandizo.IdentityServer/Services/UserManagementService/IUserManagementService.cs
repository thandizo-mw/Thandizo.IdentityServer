using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Identity.DataTransfer;

namespace Thandizo.IdentityServer.Services
{
    public interface IUserManagementService
    {
        Task<OutputResponse> RegisterUserAsync(UserDTO userDTO);
        Task<OutputResponse> UpdatePasswordAsync(PasswordResetDTO passwordResetDTO);
    }
}
