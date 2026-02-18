using System.ComponentModel.DataAnnotations;
namespace SalesForceSync.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        //SalesForce contact id
        public string? SalesForceId { get; set; }
        //contact info
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }

        // to track last modified
        public DateTime LastModifiedDate { get; set; }
        public DateTime LastSyncedAt { get; set; }
    }
}