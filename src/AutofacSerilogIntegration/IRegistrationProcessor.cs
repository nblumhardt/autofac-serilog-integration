using System;
using System.Reflection;
using Autofac.Core;

namespace AutofacSerilogIntegration
{
    public interface IRegistrationProcessor
    {
        Type Process(IComponentRegistration registration, out bool injectParameter, out PropertyInfo[] targetProperties);
    }
}