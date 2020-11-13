using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core.Resolving.Pipeline;
using Serilog;

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
}