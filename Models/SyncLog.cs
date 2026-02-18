namespace SalesForceSync.Models
{
    public class SyncLog
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int RecordsSynced { get; set; }
        public string Status { get; set; } = "";
        public string? ErrorMessage { get; set; }
    }
}