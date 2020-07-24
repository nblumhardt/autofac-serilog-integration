using System;
using System.Linq;
using Autofac;
using Autofac.Core;
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

        private void ResolveInstance<TDependency>(Action<ContainerBuilder> configureContainer, bool? alwaysSupplyParameter)
        {
            var containerBuilder = new ContainerBuilder();

            if (alwaysSupplyParameter == null)
                containerBuilder.RegisterLogger(_logger.Object);
            else
                containerBuilder.RegisterLogger(_logger.Object, alwaysSupplyParameter: alwaysSupplyParameter.Value);

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
        public void ReflectionActivator_DependencyWithLogger_ShouldCreateLogger(bool? alwaysSupplyParameter)
        {
            ResolveInstance<DependencyWithLogger>(containerBuilder => containerBuilder.RegisterType<DependencyWithLogger>(), alwaysSupplyParameter);
            VerifyLoggerCreation<DependencyWithLogger>(Times.AtLeastOnce);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        [InlineData(true)]
        public void ReflectionActivator_DependencyWithoutLogger_ShouldNotCreateLogger(bool? alwaysSupplyParameter)
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.RegisterType<DependencyWithoutLogger>(), alwaysSupplyParameter);
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        [InlineData(true)]
        public void ProvidedInstanceActivator_ShouldNotCreateLogger(bool? alwaysSupplyParameter)
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.RegisterInstance(new DependencyWithoutLogger()), alwaysSupplyParameter);
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        public void DelegateActivator_ShouldNotCreateLogger(bool? alwaysSupplyParameter)
        {
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.Register(_ => new DependencyWithoutLogger()), alwaysSupplyParameter);
            VerifyLoggerCreation<DependencyWithoutLogger>(Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        public void DelegateActivator_ShouldNotPassParameter(bool? alwaysSupplyParameter)
        {
            Parameter[] parameters = null;
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.Register((_, pp) =>
            {
                parameters = pp.ToArray();
                return new DependencyWithoutLogger();
            }), alwaysSupplyParameter);
            Assert.NotNull(parameters);
            Assert.Empty(parameters);
        }

        [Theory]
        [InlineData(true)]
        public void DelegateActivator_WhenForced_ShouldPassParameter(bool? alwaysSupplyParameter)
        {
            Parameter[] parameters = null;
            ResolveInstance<DependencyWithoutLogger>(containerBuilder => containerBuilder.Register((_, pp) =>
            {
                parameters = pp.ToArray();
                return new DependencyWithoutLogger();
            }), alwaysSupplyParameter);
            Assert.NotNull(parameters);
            var value = Assert.Single(parameters
                .OfType<TypedParameter>()
                .Where(p => p.Type == typeof(ILogger))
            )?.Value;
            Assert.IsAssignableFrom<ILogger>(value);
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
