# Eventing persistence

SQL Server code-first model for tenants and their accounts. Tenant codes are unique;
account codes are unique within a tenant. Both code fields are required, limited to
128 characters, and use a case-insensitive, accent-sensitive SQL collation. Deleting
a tenant that still has accounts is restricted. `Account.Tenant` is a string foreign
key referencing the unique alternate key `Tenant.CodeName`; there is no tenant
navigation property or tenant ID on `Account`.

Register the context and repositories in the host (with a reference to Eventing):

```csharp
using Boovatech.Eventing.Data;

builder.Services.AddEventingData(
    builder.Configuration.GetConnectionString("Eventing")
        ?? throw new InvalidOperationException("Missing Eventing connection string."),
    leaseDuration: TimeSpan.FromMinutes(1));
```

The context and repositories are scoped; do not share them across concurrent operations.
Supply the connection string through configuration or secrets.

Both repositories use static caches for `GetAllAsync`, shared across all repository
instances in the process. Each cache loads once, with no expiration. Concurrent first
reads share one load, and failed loads are retried on the next read. Each read returns
copies of the cached entities. Restart the process to pick up database changes.

Once existing solution compilation errors are resolved, create the initial migration
from the solution root using the EF CLI:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.33
$env:ConnectionStrings__Eventing = '<SQL Server connection string>'
dotnet ef migrations add InitialTenantsAndAccounts --project Eventing --startup-project Eventing --output-dir Data/Migrations
dotnet ef database update --project Eventing --startup-project Eventing
```

No migration has been generated yet. No database is created or migrated automatically
at startup.

## Inbox messages

`EventingDbContext.InboxMessages` maps messages to `InboxMessages`, with a SQL Server
rowversion `Token` and an index over unprocessed message leases. Include this entity
in the next migration before using the store. Entry IDs are generated decimal values
that map to `ulong` in the application.

`InboxMessagesStore` prefetches unprocessed messages by entry ID and atomically claims
an available message using its prefetched rowversion. The caller must supply a
`leaseDuration` through `AddEventingData` or the store constructor. Processing
must finish within that lease. Failed or abandoned work becomes available after expiry
while retries remain. The optional `retriesCount` constructor argument defaults to 3,
allowing one initial attempt plus three retries. Set it to 0 to allow only the initial
attempt. The store checks `PeekTimes` during both prefetch and the atomic claim.
Exhausted messages remain unprocessed for inspection but cannot be claimed again.

The store receives a `Func<EventingDbContext>` backed by EF's context factory. Each
database operation creates and disposes its own context. The claim update and
readback run in one transaction on that method's context.

Pass the `MessageEntry` returned by `TryLockMessageAsync` to `MarkAsProcessed`, rather
than the prefetched entry: claiming changes the rowversion. Completion checks that
token and lease expiry before recording success or rejection. No claimed-token
dictionary is retained, so completion can use another store instance. Lost ownership
raises `DbUpdateConcurrencyException`. This provides at-least-once delivery, so
handlers must tolerate retries. The existing processor still needs its
acknowledgement/rejection callbacks wired to `MarkAsProcessed`.
