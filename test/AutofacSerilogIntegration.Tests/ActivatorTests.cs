using System;
using Autofac;
using Moq;
using Serilog;
using Xunit;

namespace AutofacSerilogIntegration.Tests
{
    public class ActivatorTests
    {
        private readonly Mock<ILogger> _logger;

        public ActivatorTests()
        {
            _logger = new Mock<ILogger>();
            _logger.SetReturnsDefault(_logger.Object);
        }

        private void ResolveInstance<TDependency>(Action<ContainerBuilder> configureContainer)
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<Component<TDependency>>();
            containerBuilder.RegisterLogger(_logger.Object);
            configureContainer(containerBuilder);
            using (var container = containerBuilder.Build())
            {
                container.Resolve<Component<TDependency>>();
            }
        }

        private void VerifyLoggerCreation<TContext>(Func<Times> times)
            => _logger.Verify(logger => logger.ForContext(typeof(TContext)), times);

        [Fact]
        public void Default_ReflectionActivator_DependencyWithLogger_ShouldCreateLogger()
        {
            ResolveInstance<DependencyWithLogger>(containerBuilder => containerBuilder.RegisterType<DependencyWithLogger>());
            VerifyLoggerCreation<DependencyWithLogger>(Times.AtLeastOnce);
        }

        [Fact]
        public void Default_ReflectionActivator_DependencyWithoutLogger_ShouldNotCreateLogger()
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.RegisterType<DependencyWithoutLogger>());
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.Never);
        }

        [Fact]
        //legacy behavior
        public void Default_ProvidedInstanceActivator_DependencyWithoutLogger_CreatesLogger()
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.RegisterInstance(new DependencyWithoutLogger()));
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.AtLeastOnce);
        }

        [Fact]
        //legacy behavior
        public void Default_DelegateActivator_DependencyWithoutLogger_CreatesLogger()
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.Register(_ => new DependencyWithoutLogger()));
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.AtLeastOnce);
        }

        private class Component<TDependency>
        {
            public Component(TDependency dependency)
            {
            }
        }

        private class DependencyWithLogger
        {
            public DependencyWithLogger(ILogger logger)
            {
            }
        }

        private class DependencyWithoutLogger {}
    }
}
