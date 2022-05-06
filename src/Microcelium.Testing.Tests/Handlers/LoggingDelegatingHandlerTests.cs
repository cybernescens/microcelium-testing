using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Logging;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Microcelium.Testing.Handlers;

[RequireWebEndpoint]
internal class LoggingDelegatingHandlerTests :
  IRequireWebHostOverride, 
  IRequireLogValidation,
  IRequireLogging, 
  IConfigureServices, 
  IRequireServices
{
  private string content;

  public LogValidationContext LogContext { get; set; }

  public void Configure(WebApplication endpoint)
  {
    endpoint.MapGet(
      "/",
      context => {
        context.Response.Headers.Add("X-MICROCELIUM-RESPONSE", nameof(ResponseIsWrittenToTraceListener));
        return context.Response.WriteAsync("{ \"Message\": \"This is the response\" }");
      });
  }

  [SetUp]
  public async Task Setup()
  {
    var factory = Provider.GetRequiredService<IHttpClientFactory>();

    using (var httpClient = factory.CreateClient("logging-delegate"))
    {
      httpClient.DefaultRequestHeaders.Add("X-MICROCELIUM-REQUEST", $"{nameof(RequestIsWrittenToTraceListener)}");
      using (var response = await httpClient.GetAsync("?ignore=1"))
      {
        content = await response.Content.ReadAsStringAsync();
      }
    }
  }

  [Test]
  public void RequestIsWrittenToTraceListener()
  {
    LogContext.Received("Method         : GET");
    LogContext.Received($"RequestUri     : '{HostUri}?ignore=1'");
    LogContext.Received("Version        : 1.1");
    LogContext.Received("Method         : GET");
    LogContext.Received("Content        : <null>");
    LogContext.Received("Headers        : [ X-MICROCELIUM-REQUEST: RequestIsWrittenToTraceListener ]");
  }

  [Test]
  public void ResponseIsWrittenToTraceListener()
  {
    LogContext.Received("StatusCode     : 200");
    LogContext.Received("ReasonPhrase   : 'OK'");
    LogContext.Received("Version        : 1.1");
    LogContext.Received("Content        : System.Net.Http.HttpConnectionResponseContent");
    LogContext.Received("X-MICROCELIUM-RESPONSE: ResponseIsWrittenToTraceListener", mode: MatchMode.Contains);
  }

  [Test]
  public void ResponseContentCanStillBeRead() => content.Should().Be("{ \"Message\": \"This is the response\" }");

  public IHost Host { get; set; }
  public ILoggerFactory LoggerFactory { get; set; }
  public WebApplication Endpoint { get; set; }
  public Uri HostUri { get; set; }

  public void Apply(HostBuilderContext context, IServiceCollection services)
  {
    services.AddTransient<LoggingDelegatingHandler>();
    services
      .AddHttpClient("logging-delegate", client => client.BaseAddress = HostUri)
      .AddHttpMessageHandler(
        sp => new LoggingDelegatingHandler(sp.GetRequiredService<ILoggerFactory>()) { IncludeContents = false });
  }

  public IServiceProvider Provider { get; set; }
}