using Serilog;

namespace AutofacSerilogIntegration.Tests.SourceContext.Scenarios
{
    class AcceptsLogViaProperty : IAcceptsLogViaProperty
    {
        public ILogger Log { get; set; }

        public void CreateLog()
        {
            Log.Information("Hello, also!");
        }
    }

    interface IAcceptsLogViaProperty : ILogScenario
    {
    }
}