using System.Linq;
using Autofac;
using AutofacSerilogIntegration.Tests.SourceContext.Scenarios;
using Serilog.Sinks.TestCorrelator;
using Shouldly;
using Xunit;

namespace AutofacSerilogIntegration.Tests.SourceContext
{
    public class LoggerInjectViaConstructor : SourceContextBaseTest
    {
        [Fact]
        public void HasSourceContextProperty()
        {
            using (TestCorrelator.CreateContext())
            {
                var test = Container.Resolve<IAcceptsLogViaCtor>();
                test.CreateLog();

                var ctx = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                ctx.Count.ShouldBe(1);
                var logEvent = ctx.First();
                logEvent.Properties.ShouldContain(p => p.Key.Equals(SourceContextKey));
                logEvent.Properties[SourceContextKey].ToString().ShouldBe($"\"{typeof(AcceptsLogViaCtor).FullName}\"");
            }
        }
    }
}