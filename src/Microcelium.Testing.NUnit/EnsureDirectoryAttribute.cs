using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microcelium.Testing.Selenium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing;

public class EnsureDirectoryAttribute : RequireHostAttribute
{
  private readonly bool purge;
  private readonly string name;

  private IRequireDirectory fixture;
  private MethodInfo getMethod;
  private MethodInfo setMethod;
  private string? directoryRelative;
  private PropertyInfo property;

  public EnsureDirectoryAttribute(bool purge, string name)
  {
    this.purge = purge;
    this.name = name;
  }

  protected override IRequireHost Fixture => fixture;

  protected override void EnsureFixture(ITest test)
  {
    fixture = EnsureFixture<EnsureDirectoryAttribute<TDirectory>, TDirectory>(test);

    property = properties[0];
    getMethod = properties[0].GetMethod!;
    setMethod = properties[0].SetMethod!;
  }

  protected override void OnStartBeforeTest(ITest test)
  {
    var o = getMethod.Invoke(test.Fixture, Array.Empty<object>());
    directoryRelative = o == null ? null : Convert.ToString(o);
  }

  protected override IHostBuilder CreateHostBuilder() => new HostBuilder();

  protected override IHost CreateHost(IHostBuilder builder) => builder.Build();

  protected override void DefaultServicesConfiguration(HostBuilderContext ctx, IServiceCollection services)
  {
    
  }

  protected override void OnAfterCreateHost(ITest test)
  {
    var d = this.serviceScope!.ServiceProvider.GetRequiredService<DirectoryProvider>().GetDirectory();

    try
    {
      if (purge && Directory.Exists(d))
        Directory.Delete(d, true);

      Directory.CreateDirectory(d);
    }
    catch (Exception e)
    {
      this.loggerFactory!.CreateLogger<EnsureDirectoryAttribute>()
        .LogError(e, "Unable to delete and create {Directory} Swallowing exception...", d);
    }

    try
    {
      setMethod.Invoke(test.Fixture, new object?[] { d });
    }
    catch (Exception e)
    {
      this.loggerFactory!.CreateLogger<EnsureDirectoryAttribute>()
        .LogError(e, "Unable to set Fixture.{Property} to {Directory} ...", property.Name, d);
    }
  }

  protected override void ApplyToContext() { }
}