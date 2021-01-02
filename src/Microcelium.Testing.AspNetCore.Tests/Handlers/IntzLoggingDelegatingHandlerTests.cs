using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.AspNetCore.Handlers;
using Microcelium.Testing.Logging;
using Microcelium.Testing.NUnit.AspNetCore.TestServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog.Extensions.Logging;

namespace Microcelium.Testing.AspNetCore.Tests
{
  [Parallelizable(ParallelScope.None)]
  internal class MicroceliumLoggingDelegatingHandlerTests : IRequireTestEndpointOverride, IRequireLogValidation
  {
    private string content;

    [SetUp]
    public async Task Setup()
    {
      var factory = new SerilogLoggerFactory();
      var log = factory.CreateLogger<MicroceliumLoggingDelegatingHandlerTests>();
      using (var tracingHandler = new MicroceliumLoggingDelegatingHandler(Endpoint.CreateHandler(), log))
      using (var httpClient = new HttpClient(tracingHandler) {BaseAddress = new Uri("http://localhost")})
      {
        httpClient.DefaultRequestHeaders.Add("X-MICROCELIUM-REQUEST", $"{nameof(RequestIsWrittenToTraceListener)}");
        using (var response = await httpClient.GetAsync("?ignore=1"))
        {
          content = await response.Content.ReadAsStringAsync();
        }
      }
    }

    public TestServer Endpoint { get; set; }
    public LogTestContext LogContext { get; set; }

    public Task ServerRun(HttpContext context)
    {
      context.Response.Headers.Add("X-MICROCELIUM-RESPONSE", nameof(ResponseIsWrittenToTraceListener));
      return context.Response.WriteAsync("{ \"Message\": \"This is the response\" }");
    }

    [Test]
    public void RequestIsWrittenToTraceListener()
    {
      LogContext.Received("Method         : GET");
      LogContext.Received("RequestUri     : 'http://localhost/?ignore=1'");
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
      LogContext.Received("Content        : System.Net.Http.StreamContent");
      LogContext.Received("Headers        : [ X-MICROCELIUM-RESPONSE: ResponseIsWrittenToTraceListener ]");
    }

    [Test]
    public void ResponseContentCanStillBeRead()
      => content.Should().Be("{ \"Message\": \"This is the response\" }");

  }
}