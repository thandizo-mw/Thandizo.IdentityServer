using System.Threading.Tasks;

namespace Thandizo.IdentityServer.Services.Messaging
{
    public interface ISMSService
    {
        Task SendSmsAsync(string number, string message)
    }
}
