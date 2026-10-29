using Boovatech.Domain;
using Boovatech.Domain.Entities;

namespace Boovatech.Eventing.EventValidation
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
