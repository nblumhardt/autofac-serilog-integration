using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Example.WebApp.Services
{
    public class LoggingBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;

        public LoggingBackgroundService(ILogger logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("From background service");
            return Task.CompletedTask;
        }
    }
}