
## The IBatchAsync interface

The `IBatchASync` interface is common for any batch processor:

```cs
namespace BatchAsyncApp;

public interface IBatchAsync
{
    List<Task> Tasks { get; }

    Task ForEachBatchAsync<T>(IEnumerable<T> source, int startInclusive, int endExclusive, int batchSize, CancellationToken cancellationToken);
    Task<bool> RunPart<T>(IEnumerable<T> sourceBatch, int batchIndex, int batchStartInclusive, int batchEndExclusive, CancellationToken cancellationToken);
}
```

## Sample usage

The following example shows how to inject an implementation of `BatchAsync`:

```cs

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
    public static IBatchAsync? GetBatchAsync(IServiceProvider services) =>
        services.GetService<IBatchAsync>();
}

```

