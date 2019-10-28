# Contextual logger injection for Autofac [![Build status](https://ci.appveyor.com/api/projects/status/lannw2ooxpnwgdp4/branch/master?svg=true)](https://ci.appveyor.com/project/NicholasBlumhardt/autofac-serilog-integration/branch/master) [![NuGet Pre Release](https://img.shields.io/nuget/vpre/AutofacSerilogIntegration.svg)](https://nuget.org/packages/AutofacSerilogIntegration)

When using [Serilog](http://serilog.net), _contextual_ loggers attach the logging type's name to log events so they can later be found and filtered:

```csharp
var log = Log.ForContext<SomeClass>();
log.Information("This event is tagged with 'SomeClass'");
```

Applications that use IoC often accept dependencies as constructor parameters:

```csharp
public class SomeClass
{
  readonly ILogger _log;
  
  public SomeClass(ILogger log)
  {
    _log = log;
  }
  
  public void Show()
  {
    _log.Information("This is also an event from 'SomeClass'");
  }
}
```

This library configures [Autofac](http://autofac.org) to automatically configure the correct contextual logger for each class into which an `ILogger` is injected.

### Usage

First install from NuGet:

```powershell
Install-Package AutofacSerilogIntegration
```

Next, create the _root logger_:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
```

Then when configuring the Autofac container, call `RegisterLogger()`:

```csharp
var builder = new ContainerBuilder();

builder.RegisterLogger();
```

If no logger is explicitly passed to this function, the default `Log.Logger` will be used.
