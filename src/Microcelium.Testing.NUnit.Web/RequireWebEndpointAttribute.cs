﻿using System;
using Microcelium.Testing.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.Web;

public class RequireWebEndpointAttribute : RequireHostAttribute
{
  private string tempuri = null!;
  private WebApplicationBuilder webBuilder = null!;
  private IRequireWebHost webFixture = null!;

  protected override IRequireHost Fixture => webFixture;

  protected override void EnsureFixture(ITest test)
  {
    webFixture = EnsureFixture<RequireWebEndpointAttribute, IRequireWebHost>(test);
  }

  protected override void OnStartBeforeTest(ITest test)
  {
    /* our special builder */
    webBuilder = WebApplication.CreateBuilder(Array.Empty<string>());
    tempuri = $"http://localhost:{TcpPort.NextFreePort()}";
    if (test.Fixture is IConfigureWebHostAddress a)
      tempuri = a.GetHostUri();

    webFixture.HostUri = new Uri(tempuri);
    webBuilder.WebHost.UseSetting("urls", tempuri);
  }

  protected override IHostBuilder CreateHostBuilder() => webBuilder.Host;

  protected override IHost CreateHost(IHostBuilder builder)
  {
    var web = webBuilder.Build();
    webFixture.Host = web;
    return web;
  }

  protected override void OnAfterCreateHost(ITest test)
  {
    var web = (WebApplication)webFixture.Host;

    web.UseStaticFiles();
    web.UseRouting();

    if (test.Fixture is IRequireWebHostOverride o)
      o.Configure(web);

    web.RunAsync();
  }

  protected override void OnBeforeCreateHost(IHostBuilder builder, ITest test)
  {
    if (test.Fixture is IConfigureWebHost h)
      h.Configure(webBuilder);
  }

  protected override void OnStartAfterTest(ITest test) { }
}