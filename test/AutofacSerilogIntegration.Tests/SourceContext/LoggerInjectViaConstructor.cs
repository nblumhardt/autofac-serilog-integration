using System.Linq;
using System.Runtime.InteropServices;
using Autofac;
using AutofacSerilogIntegration.Tests.SourceContext.Scenarios;
using Serilog.Sinks.TestCorrelator;
using Shouldly;
using Xunit;

namespace AutofacSerilogIntegration.Tests.SourceContext
{
    public class LoggerInjectViaConstructor : SourceContextBaseTest
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HasSourceContextProperty(bool autowireProperties)
        {
            Arrange_Container(autowireProperties: autowireProperties);

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