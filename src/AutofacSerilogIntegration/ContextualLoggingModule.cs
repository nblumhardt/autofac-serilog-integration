using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Serilog;
using Module = Autofac.Module;

namespace AutofacSerilogIntegration
{
    public class SerilogMiddleware : IResolveMiddleware
    {
        public PipelinePhase Phase => PipelinePhase.ParameterSelection;

        private readonly Type _limitType;
        readonly ILogger _logger;
        readonly bool _autowireProperties;

        public SerilogMiddleware(Type limitType, ILogger logger, bool autowireProperties)
        {
            _limitType = limitType;
            _logger = logger;
            _autowireProperties = autowireProperties;
        }

        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {

            // Add our parameters.
            var baseLogger = _logger ?? context.Resolve<ILogger>();
            var loggerToInject = baseLogger.ForContext(_limitType);
            context.ChangeParameters(new[] { TypedParameter.From(loggerToInject) }.Concat(context.Parameters));

            // Continue the resolve.
            next(context);

            // Has an instance been activated?
            if (context.NewInstanceActivated)
            {
                if (_autowireProperties)
                {
                    var instanceType = context.Instance.GetType();

                    // Get all the injectable properties to set.
                    // If you wanted to ensure the properties were only UNSET properties,
                    // here's where you'd do it.
                    var properties = instanceType
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.PropertyType == typeof(ILogger) && p.CanWrite &&
                                    p.GetIndexParameters().Length == 0);

                    // Set the properties located.
                    foreach (var propToSet in properties)
                    {
                        propToSet.SetValue(context.Instance, baseLogger.ForContext(instanceType), null);
                    }
                }
            }
        }
    }

    // Adds a piece of middleware to every registration.
    public class MiddlewareModule : Autofac.Module
    {
        private readonly IResolveMiddleware middleware;

        public MiddlewareModule(IResolveMiddleware middleware)
        {
            this.middleware = middleware;
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistryBuilder, IComponentRegistration registration)
        {
            // Attach to the registration's pipeline build.
            registration.PipelineBuilding += (sender, pipeline) =>
            {
                // Add our middleware to the pipeline.
                pipeline.Use(middleware);
            };
        }
    }

    internal class ContextualLoggingModule : Module
    {
        const string TargetTypeParameterName = "Autofac.AutowiringPropertyInjector.InstanceType";

        readonly ILogger _logger;
        readonly bool _autowireProperties;
        readonly bool _skipRegistration;
        readonly bool _dispose;
        readonly bool _alwaysSupplyParameter;

        [Obsolete("Do not use this constructor. This is required by the Autofac assembly scanning")]
        public ContextualLoggingModule()
        {
            // Workaround to skip the logger registration when module is loaded by Autofac assembly scanning
            _skipRegistration = true;
        }

        internal ContextualLoggingModule(ILogger logger = null, bool autowireProperties = false, bool dispose = false, bool alwaysSupplyParameter = false)
        {
            _logger = logger;
            _autowireProperties = autowireProperties;
            _dispose = dispose;
            _alwaysSupplyParameter = alwaysSupplyParameter;
            _skipRegistration = false;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (_skipRegistration)
                return;

            if (_dispose)
            {
                builder.Register(c =>
                {
                    LoggerProvider provider = new LoggerProvider(_logger);
                    return provider;
                })
                    .AsSelf()
                    .AutoActivate()
                    .SingleInstance();

                builder.Register((c, p) =>
                {
                    var logger = c.Resolve<LoggerProvider>().GetLogger();

                    var targetType = p.OfType<NamedParameter>()
                        .FirstOrDefault(np => np.Name == TargetTypeParameterName && np.Value is Type);

                    if (targetType != null)
                        return logger.ForContext((Type)targetType.Value);

                    return logger;
                })
                    .As<ILogger>()
                    .ExternallyOwned();
            }
            else
            {
                builder.Register((c, p) =>
                {
                    var targetType = p.OfType<NamedParameter>()
                        .FirstOrDefault(np => np.Name == TargetTypeParameterName && np.Value is Type);

                    if (targetType != null)
                        return (_logger ?? Log.Logger).ForContext((Type)targetType.Value);

                    return _logger ?? Log.Logger;
                })
                    .As<ILogger>()
                    .ExternallyOwned();
            }
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry,
            IComponentRegistration registration)
        {
            if (_skipRegistration)
                return;

            // Ignore components that provide loggers (and thus avoid a circular dependency below)
            if (registration.Services.OfType<TypedService>().Any(ts => ts.ServiceType == typeof(ILogger) || ts.ServiceType == typeof(LoggerProvider)))
                return;

            PropertyInfo[] targetProperties = null;

            switch (registration.Activator)
            {
                case ReflectionActivator ra:
                    // As of Autofac v4.7.0 "FindConstructors" will throw "NoConstructorsFoundException" instead of returning an empty array
                    // See: https://github.com/autofac/Autofac/pull/895 & https://github.com/autofac/Autofac/issues/733
                    ConstructorInfo[] ctors;
                    try
                    {
                        ctors = ra.ConstructorFinder.FindConstructors(ra.LimitType);
                    }
                    catch (NoConstructorsFoundException)
                    {
                        ctors = new ConstructorInfo[0];
                    }

                    var usesLogger =
                        ctors.SelectMany(ctor => ctor.GetParameters()).Any(pi => pi.ParameterType == typeof(ILogger));

                    if (_autowireProperties)
                    {
                        var logProperties = ra.LimitType
                            .GetRuntimeProperties()
                            .Where(c => c.CanWrite && c.PropertyType == typeof(ILogger) && c.SetMethod.IsPublic && !c.SetMethod.IsStatic)
                            .ToArray();

                        if (logProperties.Any())
                        {
                            targetProperties = logProperties;
                            usesLogger = true;
                        }
                    }

                    // Ignore components known to be without logger dependencies
                    if (!usesLogger)
                        return;
                    break;
                case ProvidedInstanceActivator _:
                    //we cannot and should not affect provided instances
                    return;
                default:
                    //most likely a DelegateActivator - or a custom one
                    if (_alwaysSupplyParameter)
                        break;
                    else
                        return;
            }

            // Attach to the registration's pipeline build.
            registration.PipelineBuilding += (sender, pipeline) =>
            {
                // Add our middleware to the pipeline.
                pipeline.Use(new SerilogMiddleware(registration.Activator.LimitType, _logger, _autowireProperties));
            };

            //registration.Preparing += (sender, args) =>
            //{
            //    var log = args.Context.Resolve<ILogger>().ForContext(registration.Activator.LimitType);
            //    args.Parameters = new[] {TypedParameter.From(log)}.Concat(args.Parameters);
            //};

            //if (targetProperties != null)
            //{
            //    registration.Activating += (sender, args) =>
            //    {
            //        var log = args.Context.Resolve<ILogger>().ForContext(registration.Activator.LimitType);
            //        foreach (var targetProperty in targetProperties)
            //        {
            //            targetProperty.SetValue(args.Instance, log);
            //        }
            //    };
            //}
        }
    }
}
