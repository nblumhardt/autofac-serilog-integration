using System;
using Autofac;
using Moq;
using Serilog;
using Xunit;

namespace AutofacSerilogIntegration.Tests
{
    public class LoggerDisposalTests
    {
        private readonly Mock<IDisposable> _disposable;
        private readonly ILogger _logger;

        public LoggerDisposalTests()
        {
            var mock = new Mock<ILogger>();
            _disposable = mock.As<IDisposable>();
            _logger = mock.Object;
        }

        private IContainer Setup(bool dispose)
        {
            var builder = new ContainerBuilder();
            builder.RegisterLogger(_logger, dispose: dispose);
            var container = builder.Build();
            return container;
        }

        private void VerifyDisposal(bool disposed)
        {
            _disposable.Verify(d => d.Dispose(), disposed ? Times.Once() : Times.Never());
        }

        [Fact]
        public void WhenNotAskedTo_ShouldNotDisposeLogger()
        {
            Setup(false).Dispose();
            {
            }
            VerifyDisposal(false);
        }

        [Fact]
        public void WhenAskedTo_ShouldDisposeLogger()
        {
            Setup(true).Dispose();
            VerifyDisposal(true);
        }

        [Fact]
        public void WhenNotAskedTo_WhenResolvedOnContainer_ShouldNotDisposeLogger()
        {
            using (var container = Setup(false))
            {
                container.Resolve<ILogger>();
            }
            VerifyDisposal(false);
        }

        [Fact]
        public void WhenAskedTo_WhenResolvedOnContainer_ShouldDisposeLogger()
        {
            using (var container = Setup(true))
            {
                container.Resolve<ILogger>();
            }
            VerifyDisposal(true);
        }

        [Fact]
        public void WhenNotAskedTo_WhenResolvedOnNestedScope_ShouldNotDisposeLogger()
        {
            using (var container = Setup(false))
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    scope.Resolve<ILogger>();
                }
                VerifyDisposal(false);
            }
            VerifyDisposal(false);
        }

        [Fact]
        public void WhenAskedTo_WhenResolvedOnNestedScope_ShouldNotDisposeAfterScopeAndDisposeOnEnd()
        {
            using (var container = Setup(true))
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    scope.Resolve<ILogger>();
                }
                VerifyDisposal(false);
            }
            VerifyDisposal(true);
        }

    }
}
