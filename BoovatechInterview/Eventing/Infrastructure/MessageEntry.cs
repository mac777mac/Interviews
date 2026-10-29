using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boovatech.Eventing.Infrastructure
{
    /// <summary>A persisted inbox message with processing lease and completion metadata.</summary>
    /// <param name="EntryId">
    /// Database-generated primary key. Identifies the inbox row and determines prefetch order.
    /// </param>
    /// <param name="PayloadJson">
    /// Original JSON payload, deserialized into a domain event by the message handler.
    /// </param>
    /// <param name="Token">
    /// SQL Server rowversion and EF concurrency token, regenerated on every row update.
    /// Claim and completion updates compare this token to prevent stale workers from modifying the row.
    /// Pass the entry returned by TryLockMessageAsync to completion, since claiming changes the token.
    /// </param>
    /// <param name="PeekTimes">
    /// Number of successful claims, including retries after lease expiry. Incremented on each claim,
    /// capped at ushort.MaxValue; prefetching alone does not increment it.
    /// The store allows one initial claim plus its configured retriesCount; exhausted messages
    /// remain unprocessed but are excluded from further prefetches and claims.
    /// </param>
    /// <param name="PeekExpAt">
    /// UTC lease expiry. Null or expired means an unprocessed message can be claimed.
    /// A successful claim sets a new expiry; completion requires an active lease and clears it.
    /// </param>
    /// <param name="ProcessedAt">
    /// UTC completion timestamp. Null means processing is unfinished; once set, the message
    /// is excluded from prefetching regardless of whether processing succeeded or was rejected.
    /// </param>
    /// <param name="Success">
    /// Completion outcome: true for success, false for rejection, and null before completion.
    /// Set by MarkAsProcessed together with ProcessedAt.
    /// </param>
    /// <param name="Reason">
    /// Optional explanation recorded at completion, such as a validation or rejection reason.
    /// </param>
    public record MessageEntry(
        ulong EntryId,
        string PayloadJson,
        byte[] Token,
        ushort PeekTimes,
        DateTimeOffset? PeekExpAt,
        DateTimeOffset? ProcessedAt,
        bool? Success,
        string? Reason);
}
