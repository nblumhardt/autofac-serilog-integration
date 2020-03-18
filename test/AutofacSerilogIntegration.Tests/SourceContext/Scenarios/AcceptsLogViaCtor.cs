using System;
using Serilog;

namespace AutofacSerilogIntegration.Tests.SourceContext.Scenarios
{
    class AcceptsLogViaCtor : IAcceptsLogViaCtor
    {
        readonly ILogger _log;

        public AcceptsLogViaCtor(ILogger log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void CreateLog()
        {
            _log.Information("Hello!");
        }
    }

    internal interface IAcceptsLogViaCtor : ILogScenario
    {
    }
}