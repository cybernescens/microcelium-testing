using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Microcelium.Testing.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;
using Serilog.Extensions.Logging;

namespace Microcelium.Testing.Web.Http.ExceptionHandling
{
  internal class LogExceptionLoggerFixtures : IRequireLogValidation
  {
    public LogTestContext LogContext { get; set; }

    [SetUp]
    public async Task SetUp()
    {
      using (var server = TestServer.Create(
        app => {
          var factory = new SerilogLoggerFactory();
          var log = factory.CreateLogger<LogExceptionLoggerFixtures>();
          var config = new HttpConfiguration();
          config.Services.Add(typeof(IExceptionLogger), new MicroceliumExceptionLogger(log));
          config.Services.Add(typeof(IExceptionLogger), new LocalLogger());
          config.Services.Replace(typeof(IHttpControllerSelector), new HttpControllerSelector(config));
          config.MapHttpAttributeRoutes();
          config.EnsureInitialized();
          app.UseWebApi(config);
        }))
      using (var client = new HttpClient(server.Handler) {BaseAddress = new Uri("http://test")})
      using (await client.GetAsync("api/exception")) { }
    }

    private class LocalLogger : IExceptionLogger
    {
      public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
      {
        loggedException = context.ExceptionContext.Exception;
        return Task.CompletedTask;
      }
    }

    private static Exception loggedException;

    [Test]
    public void ResponseIsWrittenToMicroceliumLogListener() =>
      LogContext.Received("An exception has occurred.", LogLevel.Error, MatchMode.Exact, loggedException);

    private class HttpControllerSelector : IHttpControllerSelector
    {
      private readonly HttpConfiguration httpConfiguration;

      public HttpControllerSelector(HttpConfiguration httpConfiguration)
      {
        this.httpConfiguration = httpConfiguration;
      }

      public HttpControllerDescriptor SelectController(HttpRequestMessage request) =>
        new HttpControllerDescriptor(httpConfiguration, nameof(ExceptionController), typeof(ExceptionController));

      public IDictionary<string, HttpControllerDescriptor> GetControllerMapping() 
        => new Dictionary<string, HttpControllerDescriptor> {
          { nameof(ExceptionController),
            new HttpControllerDescriptor(httpConfiguration, nameof(ExceptionController), typeof(ExceptionController)) }
        };
    }

    private class ExceptionController : ApiController
    {
      [HttpGet]
      [Route("api/exception")]
      public object Get() => throw new Exception("Unexpected exception");
    }
  }
}