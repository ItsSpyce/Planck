namespace Planck.Services
{
  public interface IBackgroundTaskQueue
  {
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
  }

  public interface IBackgroundTaskQueue<TResult>
  {
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask<TResult>> workItem);
    ValueTask<Func<CancellationToken, ValueTask<TResult>>> DequeueAsync(CancellationToken cancellationToken);
  }
}
