using Autofac;
using AutofacSerilogIntegration.Tests.SourceContext.Scenarios;
using Serilog;

namespace AutofacSerilogIntegration.Tests.SourceContext
{
    public class SourceContextBaseTest
    {
        protected const string SourceContextKey = "SourceContext";
        protected IContainer Container;

        protected void Arrange_Container(bool autowireProperties = true)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
            var builder = new ContainerBuilder();
            builder.RegisterLogger(autowireProperties: autowireProperties);
            builder.RegisterType<AcceptsLogViaCtor>().As<IAcceptsLogViaCtor>();
            builder.RegisterType<AcceptsLogViaProperty>().As<IAcceptsLogViaProperty>();
            Container = builder.Build();
        }
    }
}