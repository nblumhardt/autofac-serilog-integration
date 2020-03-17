using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Example.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InitLogger(args);

            var host = BuildHost(args);
            Log.Information("Log started");
            host.Run();
        }

        static IHost BuildHost(string[] args) => GetHostBuilder(args).Build();

        internal static IHostBuilder GetHostBuilder(string[] args, bool loadLocalSettings = true)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(
                    config => { })
                .ConfigureWebHostDefaults(
                    config =>
                    {
                        config.ConfigureKestrel(x => x.AddServerHeader = false)
                            .UseWebRoot("Public")
                            .UseStartup<Startup>();
                    })
                .UseSerilog()
                .UseServiceProviderFactory(new AutofacServiceProviderFactory());
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });


        internal static void InitLogger(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            LoggingConfig.ConfigureServices(null);
            Log.Logger = Log.ForContext<Program>();
            Log.Logger.Information("Building Configuration completed in {Elapsed} ms", watch.ElapsedMilliseconds);
        }
    }

    public class LoggingConfig
    {
        public static void ConfigureServices(object config)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("SourceContext", null)
                .WriteTo.LiterateConsole(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message} ({SourceContext:l}){NewLine}{Exception}")
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
        }
    }
}