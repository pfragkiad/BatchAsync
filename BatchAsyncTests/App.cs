using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; //install package for DI

namespace BatchAsyncApp;

public static class App
{
    public static IHost GetHost()
    {
        var host = Host.CreateDefaultBuilder().ConfigureServices(
            (context, services) =>
            {
                //example adding the batchasync
                services.AddTransient<IBatchAsync, BatchAsync>();
            }).Build();

        return host;
    }

    //example retrieving the batchasync
    public static IBatchAsync? GetBatchAsync(this IServiceProvider services) =>
        services.GetService<IBatchAsync>();
}
