using Microsoft.AspNetCore.Mvc;
using SalesForceSync.Services;
using Microsoft.EntityFrameworkCore;
using SalesForceSync.Data;

namespace SalesForceSync.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly SalesforceContactService _contactService;
        private readonly AppDbContext _dbContext;

        public SyncController(SalesforceContactService contactService, AppDbContext dbContext)
        {
            _contactService = contactService;
            _dbContext = dbContext;
        }

        [HttpPost("contacts")]
        public async Task<IActionResult> SyncContacts()
        {
            var count = await _contactService.SyncContactsAsync();
            return Ok(new { message = $"Synced {count} contacts from Salesforce", count });
        }
        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var contacts = await _dbContext.Contacts.ToListAsync();
            return Ok(contacts);
        }
    }
}