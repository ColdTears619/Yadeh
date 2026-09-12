using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Yadeh.Application.Common.Contracts;
using Yadeh.Application.Conversations.Contracts;
using Yadeh.Infrastructure.Persistence.Repositories;

namespace Yadeh.Infrastructure.Persistence;

internal static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<YadehDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<
            IConversationRepository,
            ConversationRepository>();

        services.AddScoped<
            IUnitOfWork,
            YadehUnitOfWork>();

        return services;
    }
}