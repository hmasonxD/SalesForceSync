using Microsoft.EntityFrameworkCore;
using SalesForceSync.Models;

namespace SalesForceSync.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Contact> Contacts { get; set; }
    }
}