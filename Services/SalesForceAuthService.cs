using System.Text.Json;

namespace SalesForceSync.Services
{
    public class SalesforceAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string? _accessToken;
        private string? _instanceUrl;

        public SalesforceAuthService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public string? AccessToken => _accessToken;
        public string? InstanceUrl => _instanceUrl;

        public async Task<bool> AuthenticateAsync()
        {
            var loginUrl = _configuration["Salesforce:LoginUrl"];
            var clientId = _configuration["Salesforce:ClientId"];
            var clientSecret = _configuration["Salesforce:ClientSecret"];

            var requestBody = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("grant_type", "client_credentials"),
    new KeyValuePair<string, string>("client_id", clientId!),
    new KeyValuePair<string, string>("client_secret", clientSecret!)
});

            try
            {
                var response = await _httpClient.PostAsync($"{loginUrl}/services/oauth2/token", requestBody);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(responseString);
                    _accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString();
                    _instanceUrl = jsonDoc.RootElement.GetProperty("instance_url").GetString();

                    Console.WriteLine($"✅ Salesforce authenticated successfully!");
                    Console.WriteLine($"Instance URL: {_instanceUrl}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"❌ Salesforce authentication failed: {responseString}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Salesforce authentication error: {ex.Message}");
                return false;
            }
        }
    }
}