using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium;

[Parallelizable(ParallelScope.Fixtures)]
[RequiresWebEndpoint]
internal class ImportingNetCookies : IRequireWebHostOverride, IRequireLogging, IConfigureWebHostAddress, IConfigureServices, IRequireServices
{
  private string? tempuri;
  private string? actualCookieValue;
  private string expectedCookieValue = "Bar";
  private readonly CookieContainer cookieContainer = new();

  public void Configure(WebApplication endpoint)
  {
    endpoint.MapGet("/", context => {
      context.Response.Cookies.Append("Foo", expectedCookieValue);
      context.Response.ContentType = "text/html";
      return context.Response.WriteAsync(string.Empty);
    });

    endpoint.MapGet("/checkcookie", context => {
      actualCookieValue = context.Request.Cookies["Foo"];
      context.Response.ContentType = "text/html";
      return context.Response.WriteAsync(string.Empty);
    });
  }

  public void Apply(HostBuilderContext context, IServiceCollection services)
  {
    services
      .AddHttpClient("cookie-consumer", client => { client.BaseAddress = HostUri; })
      .ConfigurePrimaryHttpMessageHandler(
        () => new HttpClientHandler { CookieContainer = cookieContainer, UseCookies = true });
  }

  [Test]
  public async Task ImportsCookiesForDomain()
  {
    var config = new WebDriverConfig { BaseUri = HostUri.ToString() };
    var wdf = new WebDriverFactory(config);

    var factory = Provider.GetRequiredService<IHttpClientFactory>();
    
    using var inner = wdf.Create(new RuntimeConfig());
    using var driver = new WebDriverAdapter(inner, config, LoggerFactory);

    driver.Navigate().GoToUrl(HostUri);
    driver.ImportCookies(cookieContainer, HostUri);

    using var client = factory.CreateClient("cookie-consumer");
    await client.GetAsync("");

    driver.Navigate().GoToUrl(HostUri + "checkcookie");
    actualCookieValue.Should().Be(expectedCookieValue);
  }

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
  public Uri HostUri { get; set; }

  public string GetHostUri()
  {
    if (string.IsNullOrEmpty(tempuri))
      tempuri = $"https://localhost:{TcpPort.NextFreePort()}";

    return tempuri;
  }

  public IServiceProvider Provider { get; set; }
}