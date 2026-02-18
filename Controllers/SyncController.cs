using Microsoft.AspNetCore.Mvc;
using SalesForceSync.Services;
using Microsoft.EntityFrameworkCore;
using SalesForceSync.Data;
using SalesForceSync.Models;


namespace SalesForceSync.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly SalesforceContactService _contactService;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SyncController> _logger;

        public SyncController(
            SalesforceContactService contactService,
            AppDbContext dbContext,
            ILogger<SyncController> logger)
        {
            _contactService = contactService;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("contacts")]
        public async Task<IActionResult> SyncContacts()
        {
            try
            {
                _logger.LogInformation("Starting contact sync from Salesforce...");
                var count = await _contactService.SyncContactsAsync();
                _logger.LogInformation("Successfully synced {Count} contacts", count);
                return Ok(new
                {
                    success = true,
                    message = $"Synced {count} contacts from Salesforce",
                    count,
                    syncedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync contacts from Salesforce");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while syncing contacts",
                    error = ex.Message
                });
            }
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _dbContext.Contacts.AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(c =>
                        c.FirstName!.ToLower().Contains(search) ||
                        c.LastName!.ToLower().Contains(search) ||
                        c.Email!.ToLower().Contains(search) ||
                        c.Company!.ToLower().Contains(search));
                }

                var totalCount = await query.CountAsync();
                var contacts = await query
                    .OrderBy(c => c.LastName)
                    .Skip((page - 1) * pageSize) // Skip prev pages
                    .Take(pageSize) // Size desired
                    .ToListAsync();

                return Ok(new
                {
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    contacts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve contacts");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving contacts",
                    error = ex.Message
                });
            }
        }

        [HttpGet("contacts/{id}")]
        public async Task<IActionResult> GetContact(int id)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);
            if (contact == null)
                return NotFound(new { message = $"Contact with ID {id} not found" });

            return Ok(contact);
        }

        [HttpDelete("contacts/{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _dbContext.Contacts.FindAsync(id);
            if (contact == null)
                return NotFound(new { message = $"Contact with ID {id} not found" });

            _dbContext.Contacts.Remove(contact);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted contact {Id}", id);
            return Ok(new { message = $"Contact {id} deleted successfully" });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetSyncHistory()
        {
            var history = await _dbContext.SyncLogs
                .OrderByDescending(s => s.StartedAt)
                .Take(20)
                .ToListAsync();

            return Ok(history);
        }
        [HttpPost("contacts/create")]
        public async Task<IActionResult> CreateContact([FromBody] Contact contact)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contact.LastName))
                    return BadRequest(new { success = false, message = "LastName is required" });

                // Create in Salesforce first
                var salesforceId = await _contactService.CreateContactInSalesforceAsync(contact);
                if (salesforceId == null)
                    return StatusCode(500, new { success = false, message = "Failed to create contact in Salesforce" });

                // Save to local database
                contact.SalesForceId = salesforceId;
                contact.LastSyncedAt = DateTime.UtcNow;
                _dbContext.Contacts.Add(contact);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Created contact {Name} in Salesforce and database", $"{contact.FirstName} {contact.LastName}");

                return Created($"/api/sync/contacts/{contact.Id}", new
                {
                    success = true,
                    message = "Contact created in both Salesforce and local database",
                    salesforceId,
                    localId = contact.Id,
                    contact
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create contact");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}