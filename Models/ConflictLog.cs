namespace SalesForceSync.Models
{
    public class ConflictLog
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public string? SalesForceId { get; set; }

        // Old data
        public string? OldFirstName { get; set; }
        public string? OldLastName { get; set; }
        public string? OldEmail { get; set; }
        public string? OldPhone { get; set; }

        // New data
        public string? NewFirstName { get; set; }
        public string? NewLastName { get; set; }
        public string? NewEmail { get; set; }
        public string? NewPhone { get; set; }

        // Timestamps for comparison
        public DateTime LocalLastModifiedDate { get; set; }
        public DateTime SalesForceLastModifiedDate { get; set; }

        // Who won and when
        public string ResolvedBy { get; set; } = ""; // "Salesforce" or "Local"
        public DateTime DetectedAt { get; set; }
    }
}