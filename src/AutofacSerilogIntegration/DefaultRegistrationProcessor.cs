using System;
using System.Reflection;
using Autofac.Core;

namespace AutofacSerilogIntegration
{
    internal class DefaultRegistrationProcessor : IRegistrationProcessor
    {
        readonly bool _autowireProperties;

        public DefaultRegistrationProcessor(bool autowireProperties)
        {
            _autowireProperties = autowireProperties;
        }

        public Type Process(IComponentRegistration registration, out bool injectParameter, out PropertyInfo[] targetProperties)
        {
            if (!registration.Activator.TryFindLoggerDependencies(_autowireProperties, out injectParameter, out targetProperties))
            {
                //this is the legacy behavior: we skipped injection only if the answer was a definitive "no", not an "I don't know"
                injectParameter = true;
            }
            return registration.Activator.LimitType;
        }
    }
}