
# BatchAsync

The implementation of the `BatchAsync` is straightforward. All subclasses should override the `RunBatch` function:
```cs

namespace BatchAsyncApp;

public class BatchAsync : IBatchAsync
{
    //return false to stop continuation
    protected virtual async Task<bool> RunBatch<T>(
        IEnumerable<T> sourceBatch,
        int batchIndex,
        int batchStartInclusive, int batchEndExclusive,
        CancellationToken? cancellationToken = null)
    {
        Console.WriteLine($"Batch {batchIndex}: Started");
        //for (int i = 0; i < batchEndExclusive - batchStartInclusive; i++)
        int count = batchEndExclusive - batchStartInclusive; //= sourceBatch.Count();
        for (int i = 0; i < count; i++)
        {
            if (cancellationToken?.IsCancellationRequested ?? false)
            {
                Console.WriteLine($"Batch {batchIndex}: Cancelled!");
                return false;
            }

            await Task.Delay(500);
            Console.WriteLine($"Batch {batchIndex}: {sourceBatch.ElementAt(i)}");
        }
        return true;
    }

    public List<Task> Tasks { get; protected set; }

    public Task ForEachBatchAsync<T>(
        IEnumerable<T> source,
        int startInclusive, int endExclusive, int batchSize, CancellationToken? cancellationToken)
    {
        int batches =
            (endExclusive - startInclusive) % batchSize == 0 ?
            (endExclusive - startInclusive) / batchSize :
            (endExclusive - startInclusive) / batchSize + 1; //practically this is Ceiling

        Tasks = new();
        for (int iBatch = 0; iBatch < batches; iBatch++)
        {
            int batchStart = iBatch * batchSize;
            int batchEndExclusive = Math.Min((iBatch + 1) * batchSize, endExclusive);

            Task task = RunBatch(source.Skip(batchStart).Take(batchEndExclusive - batchStart),
                iBatch, batchStart, batchEndExclusive, cancellationToken);

            Tasks.Add(task);
        }

        return Task.WhenAll(Tasks);
    }

}
```

## The IBatchAsync interface

The `IBatchASync` interface is common for any batch processor (`BatchAsync` subclass)):

```cs
namespace BatchAsyncApp;

public interface IBatchAsync
{
    List<Task> Tasks { get; }

    Task ForEachBatchAsync<T>(IEnumerable<T> source, int startInclusive, int endExclusive, int batchSize, CancellationToken cancellationToken);

}
```

## Sample DI usage

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
    public static IBatchAsync? GetBatchAsync(this IServiceProvider services) =>
        services.GetService<IBatchAsync>();
}

```

## Sample usage of BatchAsync

```cs
using BatchAsyncApp;

internal class Program
{
    private static async Task Main(string[] args)
    {

        //create a sample collection
        var stuff = Enumerable.Range(0, 16).Select(i => i + 1);

        //create the processor via DI 
        var host = App.GetHost();
        var batchProcessor = host.Services.GetBatchAsync();

        //create cancel token source
        CancellationTokenSource source = new CancellationTokenSource();
        var cancelToken = source.Token;

        //we can use await to wait for everything to finish, but if we want to cancel then we can just call the function
        var task = batchProcessor.ForEachBatchAsync(stuff, 0, stuff.Count(), 3, cancelToken);

        //create a virtual delay in order to call cancel 
        await Task.Delay(1000);

        //cancel the foreach loop
        source.Cancel();

        //wait for all tasks to finish
        await task;
        
        return;
        
    }
}    
