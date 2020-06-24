using System;
using System.Reflection;
using Autofac.Core;

namespace AutofacSerilogIntegration
{
    internal class OnlyKnownCustomersRegistrationProcessor : IRegistrationProcessor
    {
        readonly bool _autowireProperties;

        public OnlyKnownCustomersRegistrationProcessor(bool autowireProperties)
        {
            _autowireProperties = autowireProperties;
        }

        public Type Process(IComponentRegistration registration, out bool injectParameter, out PropertyInfo[] targetProperties)
        {
            if (registration.Activator.TryFindLoggerDependencies(_autowireProperties, out injectParameter, out targetProperties))
                return registration.Activator.LimitType;
            else
                return null;
        }
    }
}