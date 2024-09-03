using Fantasy.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Tests.General;

public class FakeDbContext : DataContext
{
    public FakeDbContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new DbUpdateException();
    }
}