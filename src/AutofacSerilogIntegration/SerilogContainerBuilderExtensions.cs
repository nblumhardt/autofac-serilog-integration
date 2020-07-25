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
        /// <param name="dispose"></param>
        /// <param name="alwaysSupplyParameter">
        /// If true, the parameter containing <see cref="ILogger"/> will be injected even when registration cannot be verified to use it,
        /// such as <see cref="Autofac.Core.Activators.Delegate.DelegateActivator"/>.
        /// </param>
        /// <returns>An object supporting method chaining.</returns>
        public static IModuleRegistrar RegisterLogger(this ContainerBuilder builder, ILogger logger = null, bool autowireProperties = false, bool dispose = false, bool alwaysSupplyParameter = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.RegisterModule(new ContextualLoggingModule(logger, autowireProperties, dispose, alwaysSupplyParameter));
        }
        
        /// <summary>
        /// Register the <see cref="ILogger"/> with the <see cref="ContainerBuilder"/>. Where possible, the logger will
        /// be resolved using the target type as a tagged property.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="loggerLambda">Function to resolve logger from context. If resolution yeilds null, the static <see cref="Log.Logger"/> will be used. </param>
        /// <param name="autowireProperties">If true, properties on reflection-based components of type <see cref="ILogger"/> will
        /// be injected.</param>
        /// <param name="dispose"></param>
        /// <param name="alwaysSupplyParameter">
        /// If true, the parameter containing <see cref="ILogger"/> will be injected even when registration cannot be verified to use it,
        /// such as <see cref="Autofac.Core.Activators.Delegate.DelegateActivator"/>.
        /// </param>
        /// <returns>An object supporting method chaining.</returns>
        public static IModuleRegistrar RegisterLogger(this ContainerBuilder builder, Func<IComponentContext, ILogger> loggerLambda, bool autowireProperties = false, bool dispose = false, bool alwaysSupplyParameter = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.RegisterModule(new ContextualLoggingModule(loggerLambda, autowireProperties, dispose, alwaysSupplyParameter));
        }
    }
}
