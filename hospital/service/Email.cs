using hospital.models;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public interface IEmailService
{
    Task SendEmailAsync(EmailSendDto emailSendDto);
}

namespace Hospital.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Mailjet:ApiKey"];
            _apiSecret = configuration["Mailjet:ApiSecret"];
            _fromEmail = configuration["Email:From"];
            _fromName = configuration["Email:FromName"];
            _httpClient = new HttpClient();
        }

        public async Task SendEmailAsync(EmailSendDto emailSendDto)
        {
            var requestUri = "https://api.mailjet.com/v3.1/send";
            var requestContent = new JObject
            {
                ["Messages"] = new JArray
            {
                new JObject
                {
                    ["From"] = new JObject
                    {
                        ["Email"] = _fromEmail,
                        ["Name"] = _fromName
                    },
                    ["To"] = new JArray
                    {
                        new JObject
                        {
                            ["Email"] = emailSendDto.To,
                            ["Name"] = "Recipient Name"
                        }
                    },
                    ["Subject"] = emailSendDto.Subject,
                    ["TextPart"] = emailSendDto.Body,
                    ["HTMLPart"] = $"<p>{emailSendDto.Body}</p>"
                }
            }
            };

            var byteArray = Encoding.ASCII.GetBytes($"{_apiKey}:{_apiSecret}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await _httpClient.PostAsync(requestUri, new StringContent(requestContent.ToString(), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Mailjet API call failed: {responseBody}");
            }
        }
    }

}
