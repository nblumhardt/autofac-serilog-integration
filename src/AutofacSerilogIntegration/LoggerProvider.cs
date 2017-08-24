using System;
using Serilog;

namespace AutofacSerilogIntegration
{
    internal class LoggerProvider
    {
        readonly ILogger _logger;
        readonly Action _releaseAction;

        public LoggerProvider(ILogger logger = null)
        {
            _logger = logger ?? Log.Logger;
            if (logger == null)
            {
                _releaseAction = () => { Log.CloseAndFlush(); };
            }
            else
            {
                _releaseAction = () => { (_logger as IDisposable)?.Dispose(); };
            }
        }

        public ILogger GetLogger()
        {
            return _logger;
        }

        public void Release()
        {
            _releaseAction();
        }
    }
}
