using Serilog;

namespace AutofacSerilogIntegration.Tests.SourceContext.Scenarios
{
    class AcceptsLogViaProperty : IAcceptsLogViaProperty
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public ILogger Log { get; set; }

        public void CreateLog()
        {
            Log.Information("Hello, also!");
        }
    }

    internal interface IAcceptsLogViaProperty : ILogScenario
    {
    }
}