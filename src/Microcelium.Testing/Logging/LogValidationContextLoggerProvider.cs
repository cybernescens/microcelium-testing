using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Logging;

public class LogValidationContextLoggerProvider : ILoggerProvider
{
  private readonly LogValidationContext context;
  private readonly ConcurrentDictionary<string, LogValidationContextLogger> loggers = new();

  public LogValidationContextLoggerProvider(LogValidationContext context)
  {
    this.context = context;
  }

  public void Dispose()
  {
    loggers.Clear();
  }

  public ILogger CreateLogger(string categoryName)
  {
    return loggers.GetOrAdd(categoryName, x => new LogValidationContextLogger(x, context));
  }
}