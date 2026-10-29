using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boovatech.Eventing.Infrastructure
{
    public interface IInboxMessagesStore
    {
        Task<IEnumerable<MessageEntry>> PrefetchNextMessagesAsync(ushort count = 1); 
        Task<MessageEntry?> TryLockMessageAsync(MessageEntry entry);
        Task<bool> TryMarkAsProcessedAsync(MessageEntry entry, bool success, string? reason = null);
    }
}
