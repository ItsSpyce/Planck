using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Planck.Messages;

namespace Planck.Services
{
  public class BackgroundMessageService : BackgroundService
  {
    readonly IServiceProvider _services;
    readonly IBackgroundTaskQueue<MessageWorkResponse> _taskQueue;
    readonly ILogger<BackgroundMessageService> _logger;

    public BackgroundMessageService(
      IServiceProvider services,
      IBackgroundTaskQueue<MessageWorkResponse> taskQueue,
      ILogger<BackgroundMessageService> logger) =>
      (_services, _taskQueue, _logger) = (services, taskQueue, logger);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("Starting message queue");
      while (!stoppingToken.IsCancellationRequested)
      {
        // hopefully this doesn't degrade performance too bad
        var window = _services.GetRequiredService<IPlanckWindow>();
        try
        {
          var workItem = await _taskQueue.DequeueAsync(stoppingToken);
          var response = await workItem(stoppingToken);
          window.PostWebMessage(response.OperationId, response.Body);
        }
        catch (OperationCanceledException)
        {
          // ignore
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "An error occured while processing the message.");
        }
      }
    }
  }
}
