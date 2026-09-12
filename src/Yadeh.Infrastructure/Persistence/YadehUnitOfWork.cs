using Yadeh.Application.Common.Contracts;

namespace Yadeh.Infrastructure.Persistence;

internal sealed class YadehUnitOfWork(
    YadehDbContext dbContext)
    : IUnitOfWork
{
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}