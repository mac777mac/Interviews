namespace Boovatech.Domain.Entities
{
    public class AccountJournalEntry
    {
        public int EntryId { get; set; }
        public string AccountCodeName { get; set; }
        public string Tenant { get; set; }
        public int Amount { get; set; }
        public DateTimeOffset OccuredAt { get; set; }
        public string EventId { get; set; }
    }
}
