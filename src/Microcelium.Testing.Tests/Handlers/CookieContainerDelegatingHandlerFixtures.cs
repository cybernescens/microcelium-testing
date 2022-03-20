using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.Handlers;

[Parallelizable(ParallelScope.None)]
[RequireWebEndpoint]
internal class CookieContainerDelegatingHandlerFixtures : IRequireWebHostOverride, IConfigureServices, IRequireServices
{
  private CookieContainer container = new();
  private (string key, string value)[] receivedCookies;

  public void Configure(WebApplication endpoint)
  {
    endpoint.MapGet(
      "/",
      context => {
        receivedCookies = context.Request.Cookies.Select(x => (x.Key, x.Value)).ToArray();
        context.Response.Cookies.Append("foo", "bar");
        context.Response.Cookies.Append("wibble", "wobble");
        return context.Response.WriteAsync("hello world");
      });
  }
  
  public void Apply(HostBuilderContext context, IServiceCollection services)
  {
    services.AddTransient<LoggingDelegatingHandler>();
    services
      .AddHttpClient(
        "cookie-delegate",
        client => { client.BaseAddress = HostUri; })
      .ConfigurePrimaryHttpMessageHandler(
        () => new HttpClientHandler {
          UseCookies = true, 
          CookieContainer = container, 
          ClientCertificateOptions = ClientCertificateOption.Automatic
        })
      .AddHttpMessageHandler(
        sp => new LoggingDelegatingHandler(sp.GetRequiredService<ILoggerFactory>()) { IncludeContents = false });
  }

  [SetUp]
  public async Task SetUp()
  {
    var factory = Provider.GetRequiredService<IHttpClientFactory>();

    container = new CookieContainer();
    container.Add(HostUri, new Cookie("test1", "A"));
    container.Add(HostUri, new Cookie("test2", "B"));

    using (var client = factory.CreateClient("cookie-delegate"))
    using (var response = await client.GetAsync("/"))
      await response.Content.ReadAsStringAsync();
  }

  [Test]
  public void ServerReceivedCookiesFromContainer() =>
    receivedCookies
      .Should()
      .Equal(
        ("test1", "A"),
        ("test2", "B"));

  [Test]
  public void CookieContainerContainsReturnedCookiesAndOriginalCookies() =>
    container
      .GetCookies(HostUri)
      .Should()
      .Equal(
        new Cookie("test1", "A", "/", HostUri.Host),
        new Cookie("test2", "B", "/", HostUri.Host),
        new Cookie("foo", "bar", "/", HostUri.Host),
        new Cookie("wibble", "wobble", "/", HostUri.Host));

  public IHost Host { get; set; }
  public Uri HostUri { get; set; }
  public IServiceProvider Provider { get; set; }
}