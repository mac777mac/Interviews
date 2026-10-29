using Boovatech.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boovatech.Eventing.Data
{
    public static class DomainTransactionContextExtensions
    {
        public static EventingDbContext AsDbContext(this IDomainTransactionContext context) => (EventingDbContext)context;
    }
}
