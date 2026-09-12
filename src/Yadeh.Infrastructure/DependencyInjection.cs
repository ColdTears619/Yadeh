using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Yadeh.Application.SharedChats.Contracts;
using Yadeh.Infrastructure.Persistence;
using Yadeh.Infrastructure.SharedChats;
using Yadeh.Infrastructure.SharedChats.Parsing;

namespace Yadeh.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddPersistence(connectionString);

        services.AddSingleton<ReactRouterStreamDecoder>();

        services.AddSingleton<
            ISharedChatPageParser,
            ChatGptSharedChatPageParser>();

        services.AddHttpClient<
            IChatGptSharedChatProbeClient,
            ChatGptSharedChatProbeClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);

            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(
                    "Yadeh",
                    "0.1"));

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "text/html"));
        });

        return services;
    }
}