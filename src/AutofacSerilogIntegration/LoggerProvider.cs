using System;
using Serilog;

namespace AutofacSerilogIntegration
{
    internal class LoggerProvider : IDisposable
    {
        readonly ILogger _logger;
        readonly Action _disposeAction;

        public LoggerProvider(ILogger logger = null, bool dispose = false)
        {
            _logger = logger ?? Log.Logger;
            if (logger == null && dispose)
            {
                _disposeAction = () => { Log.CloseAndFlush(); };
            }
            else if (logger != null && dispose)
            {
                _disposeAction = () => { (_logger as IDisposable)?.Dispose(); };
            }
            else
            {
                _disposeAction = () => { };
            }
        }

        public ILogger GetLogger()
        {
            return _logger ?? Log.Logger;
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposeAction();
        }

        ~LoggerProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
