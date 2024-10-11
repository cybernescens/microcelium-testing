using System;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Logging;

internal class LogValidationContextLogger : ILogger
{
  private readonly string name;
  private readonly LogValidationContext context;

  public LogValidationContextLogger(string name, LogValidationContext context)
  {
    this.name = name;
    this.context = context;
  }

  public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
  {
    context.Log(name, logLevel, eventId, state, exception, formatter);
  }

  public bool IsEnabled(LogLevel logLevel) => true;

  public IDisposable BeginScope<TState>(TState state) => context.BeginScope(state);
}