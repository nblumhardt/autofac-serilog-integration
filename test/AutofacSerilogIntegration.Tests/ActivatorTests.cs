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

        private void ResolveInstance<TDependency>(Action<ContainerBuilder> configureContainer, bool? onlyKnownConsumers)
        {
            var containerBuilder = new ContainerBuilder();

            if (onlyKnownConsumers == null)
                containerBuilder.RegisterLogger(_logger.Object);
            else
                containerBuilder.RegisterLogger(_logger.Object, onlyKnownConsumers: onlyKnownConsumers.Value);

            containerBuilder.RegisterType<Component<TDependency>>();
            configureContainer(containerBuilder);
            using (var container = containerBuilder.Build())
            {
                container.Resolve<Component<TDependency>>();
            }
        }

        private void VerifyLoggerCreation<TContext>(Func<Times> times)
            => _logger.Verify(logger => logger.ForContext(typeof(TContext)), times);

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        [InlineData(true)]
        public void ReflectionActivator_DependencyWithLogger_ShouldCreateLogger(bool? onlyKnownConsumers)
        {
            ResolveInstance<DependencyWithLogger>(containerBuilder => containerBuilder.RegisterType<DependencyWithLogger>(), onlyKnownConsumers);
            VerifyLoggerCreation<DependencyWithLogger>(Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        [InlineData(true)]
        public void ReflectionActivator_DependencyWithoutLogger_ShouldNotCreateLogger(bool? onlyKnownConsumers)
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.RegisterType<DependencyWithoutLogger>(), onlyKnownConsumers);
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        //legacy behavior
        public void Default_ProvidedInstanceActivator_DependencyWithoutLogger_CreatesLogger(bool? onlyKnownConsumers)
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.RegisterInstance(new DependencyWithoutLogger()), onlyKnownConsumers);
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        //legacy behavior
        public void Default_DelegateActivator_DependencyWithoutLogger_CreatesLogger(bool? onlyKnownConsumers)
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.Register(_ => new DependencyWithoutLogger()), onlyKnownConsumers);
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.AtLeastOnce);
        }

        [Fact]
        public void OnlyKnownCustomers_ProvidedInstanceActivator_DependencyWithoutLogger_ShouldNotCreateLogger()
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.RegisterInstance(new DependencyWithoutLogger()), onlyKnownConsumers: true);
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.Never);
        }

        [Fact]
        public void OnlyKnownCustomers_DelegateActivator_DependencyWithoutLogger_ShouldNotCreateLogger()
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.Register(_ => new DependencyWithoutLogger()), onlyKnownConsumers: true);
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.Never);
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
