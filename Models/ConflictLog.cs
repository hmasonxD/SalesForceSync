namespace SalesForceSync.Models
{
    public class ConflictLog
    {
        public int Id { get; set; }
        //SalesForce contact id
        public string? SalesForceId { get; set; }
        public DateTime LocalLastModifiedDate { get; set; }
        public DateTime SalesForceLastModifiedDate { get; set; }
        public DateTime LastSyncedAt { get; set; }

    }
}