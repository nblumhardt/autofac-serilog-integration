using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Serilog;

namespace AutofacSerilogIntegration
{
    internal class ContextualLoggingModule : Module
    {
        const string TargetTypeParameterName = "Autofac.AutowiringPropertyInjector.InstanceType";

        readonly ILogger _logger;
        readonly IRegistrationProcessor _registrationProcessor;
        readonly bool _skipRegistration;
        readonly bool _dispose;

        [Obsolete("Do not use this constructor. This is required by the Autofac assembly scanning")]
        public ContextualLoggingModule()
        {
            // Workaround to skip the logger registration when module is loaded by Autofac assembly scanning
            _skipRegistration = true;
        }

        internal ContextualLoggingModule(IRegistrationProcessor registrationProcessor, ILogger logger, bool dispose)
        {
            _logger = logger;
            _registrationProcessor = registrationProcessor;
            _dispose = dispose;
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

            var source = _registrationProcessor.Process(registration, out var injectParameter, out var targetProperties);
            if (source == null)
                return;

            if (injectParameter)
            {
                registration.Preparing += (sender, args) =>
                {
                    var log = args.Context.Resolve<ILogger>().ForContext(source);
                    args.Parameters = new[] {TypedParameter.From(log)}.Concat(args.Parameters);
                };
            }

            if (targetProperties != null && targetProperties.Length > 0)
            {
                registration.Activating += (sender, args) =>
                {
                    var log = args.Context.Resolve<ILogger>().ForContext(source);
                    foreach (var targetProperty in targetProperties)
                    {
                        targetProperty.SetValue(args.Instance, log);
                    }
                };
            }
        }
    }
}
