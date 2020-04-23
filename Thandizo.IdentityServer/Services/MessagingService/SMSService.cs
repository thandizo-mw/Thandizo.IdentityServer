using AngleDimension.Standard.Http.HttpServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Thandizo.DataModels.SMS;
using Thandizo.IdentityServer.Helpers;
using Thandizo.IdentityServer.Services.Messaging;

namespace Thandizo.IdentityServer.Services.Messaging
{
    public class SMSService : ISMSService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public SMSService(
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<SMSService>();
        }

        public string SmsUrl
        {
            get
            {
                return _configuration["SmsURL"];
            }
        }

        public async Task SendSmsAsync(string number, string message)
        {
            // You can use the debug output together with return for testing without receiving a SMS message.
            System.Diagnostics.Debug.WriteLine(message);
            _logger.LogInformation(message);
            //System.Diagnostics.Debug.WriteLine($"Message sent to {number}");
            //return;
            var smsMessage = new SmsMessage()
            {
                Message = $"{message}",
                Recipients = new string[] { number },
                Sender = "Khusa",
                Source = "thandizo_web_portal"
            };

            var smsFeedback = await SendSMS(smsMessage, SmsChannel.SendAndForget);
            System.Diagnostics.Debug.WriteLine(smsFeedback);
            return;
        }

        private async Task<SmsFeedback> SendSMS(SmsMessage smsMessage, SmsChannel smsChannel)
        {
            SmsFeedback smsFeedback = new SmsFeedback();

            try
            {

                string apiEndpoint = string.Empty;

                if (smsChannel == SmsChannel.SendAndWaitForFeedback)
                {
                    apiEndpoint = "Messages/SendMessage";
                }
                else if (smsChannel == SmsChannel.SendAndForget)
                {
                    apiEndpoint = "Messages/SendMessageRapid";
                }
                
                string url = $"{SmsUrl}{apiEndpoint}";
                var response = await HttpRequestFactory.Post(url, smsMessage);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    smsFeedback.MessageStatus = 1;
                    smsFeedback.Message = "OTPSent";
                    _logger.LogInformation($"SMS sent to {smsMessage.Recipients.ToString()}. Waiting for responses");
                }
                else
                {
                    smsFeedback.MessageStatus = 0;
                    smsFeedback.Message = "OTPNotSent";
                }

            }
            catch (Exception)
            {
                smsFeedback.MessageStatus = 0;
                smsFeedback.Message = "OTPSmsError";
            }

            return smsFeedback;
        }
    }
}
