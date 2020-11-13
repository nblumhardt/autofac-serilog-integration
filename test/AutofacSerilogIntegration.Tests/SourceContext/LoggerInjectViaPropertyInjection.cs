using System.Linq;
using Autofac;
using AutofacSerilogIntegration.Tests.SourceContext.Scenarios;
using Serilog.Sinks.TestCorrelator;
using Shouldly;
using Xunit;

namespace AutofacSerilogIntegration.Tests.SourceContext
{
    public class LoggerInjectViaPropertyInjection : SourceContextBaseTest
    {
        [Fact]
        public void HasSourceContextProperty()
        {
            Arrange_Container();

            using (TestCorrelator.CreateContext())
            {
                var test = Container.Resolve<IAcceptsLogViaProperty>();
                test.CreateLog();

                var ctx = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                ctx.Count.ShouldBe(1);
                var logEvent = ctx.First();
                logEvent.Properties.ShouldContain(p => p.Key.Equals(SourceContextKey));
                logEvent.Properties[SourceContextKey].ToString()
                    .ShouldBe($"\"{typeof(AcceptsLogViaProperty).FullName}\"");
            }
        }

        [Fact]
        public void DoesNotHaveSourceContextPropertyWhenAutowireDisabled()
        {
            Arrange_Container(autowireProperties: false);

            using (TestCorrelator.CreateContext())
            {
                var test = Container.Resolve<IAcceptsLogViaProperty>();
                test.CreateLog();

                var ctx = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                ctx.ShouldBeEmpty();
            }
        }
    }
}