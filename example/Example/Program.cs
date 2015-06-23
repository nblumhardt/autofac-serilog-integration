using System;
using System.Collections.Generic;
using Autofac;
using AutofacSerilogIntegration;
using Serilog;

namespace Example
{
    interface IExample
    {
        void Show();
    }

    class AcceptsLogViaCtor : IExample
    {
        readonly ILogger _log;

        public AcceptsLogViaCtor(ILogger log)
        {
            if (log == null) throw new ArgumentNullException("log");
            _log = log;
        }

        public void Show()
        {
            _log.Information("Hello!");
        }
    }

    class AcceptsLogViaProperty : IExample
    {
        public ILogger Log { get; set; }

        public void Show()
        {
            Log.Information("Hello, also!");
        }
    }

    class Program
    {
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("SourceContext", null)
                .WriteTo.LiterateConsole(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message} ({SourceContext:l}){NewLine}{Exception}")
                .CreateLogger();

            try
            {
                var builder = new ContainerBuilder();
                builder.RegisterLogger(autowireProperties: true);
                builder.RegisterType<AcceptsLogViaCtor>().As<IExample>();
                builder.RegisterType<AcceptsLogViaProperty>().As<IExample>();

                using (var container = builder.Build())
                {
                    var examples = container.Resolve<IEnumerable<IExample>>();
                    foreach (var example in examples)
                    {
                        example.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled error");
            }
        }
    }
}
