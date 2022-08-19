namespace BatchAsyncApp;

public interface IBatchAsync
{
	List<Task> Tasks { get; }

	Task ForEachBatchAsync<T>(IEnumerable<T> source, int startInclusive, int endExclusive, int batchSize, CancellationToken? cancellationToken = null);

}