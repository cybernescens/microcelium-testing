using System;
using System.Diagnostics;
using System.Web.Http.ExceptionHandling;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Web.Http.ExceptionHandling
{
  public class MicroceliumExceptionLogger : ExceptionLogger
  {
    private readonly ILogger log;

    public MicroceliumExceptionLogger(ILogger log) { this.log = log; }

    public override void Log(ExceptionLoggerContext context) =>
      log.Log(LogLevel.Error, context.ExceptionContext.Exception.Demystify(), "An exception has occurred.");
  }
}
