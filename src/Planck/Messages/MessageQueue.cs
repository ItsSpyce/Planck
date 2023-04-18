using Planck.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Planck.Messages
{
  public sealed class MessageQueue : IBackgroundTaskQueue<MessageWorkResponse>
  {
    readonly Channel<Func<CancellationToken, ValueTask<MessageWorkResponse>>> _queue;

    public MessageQueue()
    {
      var options = new BoundedChannelOptions(256)
      {
        FullMode = BoundedChannelFullMode.Wait,
      };
      _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask<MessageWorkResponse>>>(options);
    }

    public async ValueTask<Func<CancellationToken, ValueTask<MessageWorkResponse>>> DequeueAsync(CancellationToken cancellationToken)
    {
      await _queue.Reader.WaitToReadAsync(cancellationToken);
      var workItem = await _queue.Reader.ReadAsync(cancellationToken);
      return workItem;
    }

    public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask<MessageWorkResponse>> workItem)
    {
      if (workItem is null)
      {
        throw new ArgumentNullException(nameof(workItem));
      }
      await _queue.Writer.WriteAsync(workItem);
    }
  }
}
