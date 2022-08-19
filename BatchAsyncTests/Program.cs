
using BatchAsyncApp;

namespace BatchAsyncTests;

internal  class Program
{
    static async Task Main(string[] args)
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
    }
}