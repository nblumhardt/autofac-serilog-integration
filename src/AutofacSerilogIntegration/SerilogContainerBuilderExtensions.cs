using System;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Serilog;

namespace AutofacSerilogIntegration
{
    /// <summary>
    /// Extends <see cref="ContainerBuilder"/> with registration methods for Serilog logging.
    /// </summary>
    public static class SerilogContainerBuilderExtensions
    {
        /// <summary>
        /// Register the <see cref="ILogger"/> with the <see cref="ContainerBuilder"/>. Where possible, the logger will
        /// be resolved using the target type as a tagged property.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="logger">The logger. If null, the static <see cref="Log.Logger"/> will be used.</param>
        /// <param name="autowireProperties">If true, properties on reflection-based components of type <see cref="ILogger"/> will
        /// be injected.</param>
        /// <param name="dispose"></param>
        /// <param name="onlyKnownConsumers">
        /// If true, only the registrations that can be verified to use <see cref="ILogger"/> will be modified to access the logger,
        /// thus avoiding unnecessary logger calls.
        ///  </param>
        /// <returns>An object supporting method chaining.</returns>
        public static IModuleRegistrar RegisterLogger(this ContainerBuilder builder, ILogger logger = null, bool autowireProperties = false, bool dispose = false, bool onlyKnownConsumers = false)
        {
            var registrationProcessor = onlyKnownConsumers
                ? (IRegistrationProcessor) new OnlyKnownCustomersRegistrationProcessor(autowireProperties)
                : new DefaultRegistrationProcessor(autowireProperties);
            return builder.RegisterLogger(registrationProcessor, logger, dispose);
        }

        /// <summary>
        /// Register the <see cref="ILogger"/> with the <see cref="ContainerBuilder"/>. Where possible, the logger will
        /// be resolved using the target type as a tagged property.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="registrationProcessor">A strategy to process <see cref="IComponentRegistration"/> for logging injection.</param>
        /// <param name="logger">The logger. If null, the static <see cref="Log.Logger"/> will be used.</param>
        /// <param name="dispose"></param>
        /// <returns>An object supporting method chaining.</returns>
        public static IModuleRegistrar RegisterLogger(this ContainerBuilder builder, IRegistrationProcessor registrationProcessor, ILogger logger = null, bool dispose = false)
        {
            return builder.RegisterModule(new ContextualLoggingModule(registrationProcessor, logger, dispose));
        }
    }
}
