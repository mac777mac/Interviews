

namespace Boovatech.Domain.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string CodeName { get; set; } = null!;
        public AccountType Type { get; set; }
        public int Amount { get; set; }
        public string Tenant { get; set; } = null!;
        public bool IsPhysical { get; internal set; }
    }
}
