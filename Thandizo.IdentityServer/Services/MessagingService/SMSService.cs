using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Thandizo.DataModels.SMS;
using Thandizo.IdentityServer.Helpers;

namespace Thandizo.IdentityServer.Services
{
    public class SMSService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IHttpRequestHandler _httpRequestHandler;

        public SMSService(
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IHttpRequestHandler httpRequestHandler)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<SMSService>();
            _httpRequestHandler = httpRequestHandler;
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
                Sender = "Thandizo",
                Source = "thandizo_app"
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
                var client = new HttpClient();

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
                var response = await _httpRequestHandler.Post(url, smsMessage);
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
