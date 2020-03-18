using Autofac;
using AutofacSerilogIntegration.Tests.SourceContext.Scenarios;
using Serilog;

namespace AutofacSerilogIntegration.Tests.SourceContext
{
    public class SourceContextBaseTest
    {
        protected const string SourceContextKey = "SourceContext";
        private readonly ContainerBuilder _builder;
        protected readonly IContainer Container;

        protected SourceContextBaseTest()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
            _builder = new ContainerBuilder();
            _builder.RegisterLogger(autowireProperties: true);
            _builder.RegisterType<AcceptsLogViaCtor>().As<IAcceptsLogViaCtor>();
            _builder.RegisterType<AcceptsLogViaProperty>().As<IAcceptsLogViaProperty>();
            Container = _builder.Build();
        }
    }
}