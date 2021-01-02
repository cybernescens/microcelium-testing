using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit.AspNetCore.TestServer
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
  public class RequiresTestEndpointAttribute : Attribute, ITestAction
  {
    private Microsoft.AspNetCore.TestHost.TestServer testServer;

    public void BeforeTest(ITest test)
    {
      if (!(test.Fixture is IRequireTestEndpoint requireTestEndpoint))
      {
        throw new Exception(
          $"Test should implement interface '{typeof(IRequireTestEndpoint)}'"
          + $" instead of using the attribute '{GetType()}'");
      }

      var hostConfig = test.Fixture as IRequireTestEndpointHostBuilder;
      var appConfig = test.Fixture as IRequireTestEndpointApplicationBuilder;
      var servicesConfig = test.Fixture as IRequireTestEndpointServices;
      var overrideConfig = test.Fixture as IRequireTestEndpointOverride;

      var hostBuilder = new WebHostBuilder();
      hostConfig?.Configure(hostBuilder);
      hostBuilder.Configure(x => appConfig?.Configure(x));

      if (servicesConfig != null)
        hostBuilder.ConfigureServices(servicesConfig.Configure);

      if (overrideConfig != null)
        hostBuilder.Configure(x => x.Run(overrideConfig.ServerRun));

      var startupDefinition = test.Fixture.GetType()
        .GetInterfaces()
        .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequireTestEndpointStartup<>));

      if (startupDefinition != null)
      {
        var startupType = startupDefinition.GetGenericArguments()[0];
        hostBuilder.UseStartup(startupType);
      }

      testServer = requireTestEndpoint.Endpoint =
        new Microsoft.AspNetCore.TestHost.TestServer(hostBuilder);
    }

    public void AfterTest(ITest test)
    {
      try { testServer?.Dispose(); } catch { }
    }

    public ActionTargets Targets => ActionTargets.Suite;
  }
}