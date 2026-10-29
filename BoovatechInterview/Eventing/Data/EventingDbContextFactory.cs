using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Boovatech.Eventing.Data;

public sealed class EventingDbContextFactory : IDesignTimeDbContextFactory<EventingDbContext>
{
    public EventingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Eventing");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Set ConnectionStrings__Eventing before running EF migrations.");

        var options = new DbContextOptionsBuilder<EventingDbContext>()
            .UseSqlServer(connectionString).Options;
        return new EventingDbContext(options);
    }
}
