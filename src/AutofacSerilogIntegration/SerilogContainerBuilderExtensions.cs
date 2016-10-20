using System;
using Autofac;
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
        /// <param name="autowirePrivateProperties">If true, private properties of type <see cref="ILogger"/> will be injected.</param>
        /// <returns>An object supporting method chaining.</returns>
        public static IModuleRegistrar RegisterLogger(this ContainerBuilder builder, ILogger logger = null, bool autowireProperties = false, bool autowirePrivateProperties = false)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            return builder.RegisterModule(new ContextualLoggingModule(logger, autowireProperties, autowirePrivateProperties));
        }
    }
}
