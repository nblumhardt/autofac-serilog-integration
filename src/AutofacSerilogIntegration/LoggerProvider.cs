using System;
using Serilog;

namespace AutofacSerilogIntegration
{
    internal class LoggerProvider : IDisposable
    {
        readonly ILogger _logger;
        readonly Action _disposeAction;

        public LoggerProvider(ILogger logger = null)
        {
            _logger = logger ?? Log.Logger;
            if (logger == null)
            {
                _disposeAction = () => { Log.CloseAndFlush(); };
            }
            else
            {
                _disposeAction = () => { (_logger as IDisposable)?.Dispose(); };
            }
        }

        public ILogger GetLogger()
        {
            return _logger;
        }

        public void Dispose()
        {
			_disposeAction();
		}
	}
}
