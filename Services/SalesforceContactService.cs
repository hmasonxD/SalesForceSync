using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SalesForceSync.Data;
using SalesForceSync.Models;

namespace SalesForceSync.Services
{
    public class SalesforceContactService
    {
        private readonly SalesforceAuthService _authService;
        private readonly AppDbContext _dbContext;
        private readonly HttpClient _httpClient;

        public SalesforceContactService(
            SalesforceAuthService authService,
            AppDbContext dbContext,
            HttpClient httpClient)
        {
            _authService = authService;
            _dbContext = dbContext;
            _httpClient = httpClient;
        }

        public async Task<int> SyncContactsAsync()
        {
            // Step 1: Authenticate with Salesforce
            var authenticated = await _authService.AuthenticateAsync();
            if (!authenticated)
            {
                Console.WriteLine("❌ Cannot sync - authentication failed");
                return 0;
            }

            // Step 2: Fetch contacts from Salesforce
            var contacts = await FetchContactsFromSalesforceAsync();
            if (contacts == null || contacts.Count == 0)
            {
                Console.WriteLine("No contacts found in Salesforce");
                return 0;
            }

            // Step 3: Save contacts to database
            var savedCount = await SaveContactsToDatabaseAsync(contacts);
            Console.WriteLine($"✅ Synced {savedCount} contacts from Salesforce");
            return savedCount;
        }

        private async Task<List<Contact>> FetchContactsFromSalesforceAsync()
        {
            var query = "SELECT Id, FirstName, LastName, Email, Phone FROM Contact";
            var url = $"{_authService.InstanceUrl}/services/data/v59.0/query?q={Uri.EscapeDataString(query)}";

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _authService.AccessToken);

            var response = await _httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Failed to fetch contacts: {responseString}");
                return new List<Contact>();
            }

            var jsonDoc = JsonDocument.Parse(responseString);
            var records = jsonDoc.RootElement.GetProperty("records");

            var contacts = new List<Contact>();
            foreach (var record in records.EnumerateArray())
            {
                contacts.Add(new Contact
                {
                    SalesForceId = record.GetProperty("Id").GetString() ?? "",
                    FirstName = record.TryGetProperty("FirstName", out var fn) ? fn.GetString() ?? "" : "",
                    LastName = record.TryGetProperty("LastName", out var ln) ? ln.GetString() ?? "" : "",
                    Email = record.TryGetProperty("Email", out var em) ? em.GetString() ?? "" : "",
                    Phone = record.TryGetProperty("Phone", out var ph) ? ph.GetString() ?? "" : "",
                    LastSyncedAt = DateTime.UtcNow
                });
            }

            Console.WriteLine($"📥 Fetched {contacts.Count} contacts from Salesforce");
            return contacts;
        }

        private async Task<int> SaveContactsToDatabaseAsync(List<Contact> contacts)
        {
            foreach (var contact in contacts)
            {
                var existing = await _dbContext.Contacts
                    .FirstOrDefaultAsync(c => c.SalesForceId == contact.SalesForceId);

                if (existing != null)
                {
                    // Update existing contact
                    existing.FirstName = contact.FirstName;
                    existing.LastName = contact.LastName;
                    existing.Email = contact.Email;
                    existing.Phone = contact.Phone;
                    existing.LastSyncedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new contact
                    await _dbContext.Contacts.AddAsync(contact);
                }
            }

            await _dbContext.SaveChangesAsync();
            return contacts.Count;
        }
    }
}