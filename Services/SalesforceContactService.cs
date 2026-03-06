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
            var syncLog = new SyncLog
            {
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };
            _dbContext.SyncLogs.Add(syncLog);
            await _dbContext.SaveChangesAsync();

            try
            {
                // Step 1: Authenticate with Salesforce
                var authenticated = await _authService.AuthenticateAsync();
                if (!authenticated)
                {
                    syncLog.Status = "Failed";
                    syncLog.ErrorMessage = "sales force authentication failed!";
                    syncLog.CompletedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                    // Console.WriteLine("❌ Cannot sync - authentication failed");
                    return 0;
                }

                // Step 2: Fetch contacts from Salesforce
                var contacts = await FetchContactsFromSalesforceAsync();
                if (contacts == null || contacts.Count == 0)
                {
                    syncLog.Status = "Success";
                    syncLog.RecordsSynced = 0;
                    syncLog.CompletedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                    // Console.WriteLine("No contacts found in Salesforce");
                    return 0;
                }

                // Step 3: Save contacts to database
                var savedCount = await SaveContactsToDatabaseAsync(contacts);
                syncLog.Status = "Success";
                syncLog.RecordsSynced = savedCount;
                syncLog.CompletedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"✅ Synced {savedCount} contacts from Salesforce");
                return savedCount;
            }
            catch (Exception ex)
            {
                syncLog.Status = "Failed";
                syncLog.ErrorMessage = ex.Message;
                syncLog.CompletedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                throw;
            }

        }

        private async Task<List<Contact>> FetchContactsFromSalesforceAsync()
        {
            var lastSync = await _dbContext.SyncLogs
            .Where(s => s.Status == "Success")
            .OrderByDescending(s => s.CompletedAt)
            .FirstOrDefaultAsync();
            string query;
            if (lastSync == null)
            {
                query = "SELECT Id, FirstName, LastName, Email, Phone, LastModifiedDate FROM Contact";
            }
            else
            {
                var sinceDate = lastSync.CompletedAt!.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
                query = $"SELECT Id, FirstName, LastName, Email, Phone, LastModifiedDate FROM Contact WHERE LastModifiedDate > {sinceDate}";
            }
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
                    LastSyncedAt = DateTime.UtcNow,
                    LastModifiedDate = record.TryGetProperty("LastModifiedDate", out var lmd) ? DateTime.Parse(lmd.GetString() ?? "") : DateTime.UtcNow
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
                    existing.LastModifiedDate = contact.LastModifiedDate;
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
        public async Task<string?> CreateContactInSalesforceAsync(Contact contact)
        {
            var authenticated = await _authService.AuthenticateAsync();
            if (!authenticated)
            {
                Console.WriteLine("❌ Cannot create contact - authentication failed");
                return null;
            }

            var url = $"{_authService.InstanceUrl}/services/data/v59.0/sobjects/Contact";

            var contactData = new
            {
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                Phone = contact.Phone
            };

            var json = System.Text.Json.JsonSerializer.Serialize(contactData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _authService.AccessToken);

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseString);
                var salesforceId = jsonDoc.RootElement.GetProperty("id").GetString();
                Console.WriteLine($"✅ Created contact in Salesforce with ID: {salesforceId}");
                return salesforceId;
            }
            else
            {
                Console.WriteLine($"❌ Failed to create contact in Salesforce: {responseString}");
                return null;
            }
        }
    }
}