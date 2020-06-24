using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Serilog;

namespace AutofacSerilogIntegration
{
    internal static class ActivatorExtensions
    {
        internal static bool TryFindLoggerDependencies(this IInstanceActivator activator, bool inspectProperties, out bool injectParameter, out PropertyInfo[] targetProperties)
        {
            injectParameter = false;
            targetProperties = null;
            switch (activator)
            {
                case ReflectionActivator ra:
                    // As of Autofac v4.7.0 "FindConstructors" will throw "NoConstructorsFoundException" instead of returning an empty array
                    // See: https://github.com/autofac/Autofac/pull/895 & https://github.com/autofac/Autofac/issues/733
                    ConstructorInfo[] ctors;
                    try
                    {
                        ctors = ra.ConstructorFinder.FindConstructors(ra.LimitType);
                    }
                    catch (Exception ex) when (ex.GetType().Name == "NoConstructorsFoundException"
                    ) // Avoid needing to upgrade our Autofac reference to 4.7.0
                    {
                        ctors = new ConstructorInfo[0];
                    }

                    injectParameter = ctors.SelectMany(ctor => ctor.GetParameters())
                        .Any(pi => pi.ParameterType == typeof(ILogger));

                    if (inspectProperties)
                    {
                        var logProperties = ra.LimitType
                            .GetRuntimeProperties()
                            .Where(c => c.CanWrite && c.PropertyType == typeof(ILogger) && c.SetMethod.IsPublic &&
                                        !c.SetMethod.IsStatic)
                            .ToArray();

                        if (logProperties.Any())
                        {
                            targetProperties = logProperties;
                        }
                    }

                    return true;
                default:
                    return false;
            }
        }
    }
}