using Fantasy.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Fantasy.Tests.General;

public class FakeDbContextWithGeneralException : DataContext
{
    public FakeDbContextWithGeneralException(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new Exception("General exception occurred");
    }
}