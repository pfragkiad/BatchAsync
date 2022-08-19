
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