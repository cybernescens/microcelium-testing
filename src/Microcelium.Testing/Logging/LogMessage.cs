using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Logging;

public readonly struct LogMessage 
{
  public LogMessage(LogLevel level, string message, Exception? exception = null, DateTime? timestamp = null, string? loggerName = null, params (string Key, string Value)[] properties)
  {
    LoggerName = loggerName ?? "+++++ expectation +++++";
    Level = level;
    Exception = exception;
    Properties = properties;
    LoggedMessage = message;
    Timestamp = timestamp ?? DateTime.Now;
  }

  public string LoggerName { get; }
  public LogLevel Level { get; }
  public string LoggedMessage { get; }
  public Exception? Exception { get; }
  public DateTime Timestamp { get; }

  // ReSharper disable UnusedAutoPropertyAccessor.Global
  public (string Key, string Value)[] Properties { get; }
  // ReSharper restore UnusedAutoPropertyAccessor.Global

  public override int GetHashCode() => LogMessageComparer.Default.GetHashCode(this);

  public override string ToString()
  {
    var message = LoggedMessage;
    if (Exception != null)
      message = message + "|" + Exception;

    return $"{Timestamp} | {Level} | {LoggerName} | {message}";
  }
}

public sealed class LogMessageComparer : IEqualityComparer<LogMessage>
{
  public static LogMessageComparer Default = new((x, y) => string.Equals(x, y, StringComparison.CurrentCultureIgnoreCase));
  
  private readonly Func<string, string, bool> messageComparer;

  public LogMessageComparer(Func<string, string, bool> messageComparer)
  {
    this.messageComparer = messageComparer;
  }

  public bool Equals(LogMessage x, LogMessage y) =>
    x.GetType() == y.GetType() &&
    x.Level == y.Level &&
    messageComparer(x.LoggedMessage, y.LoggedMessage) &&
    Equals(x.Exception, y.Exception);

  public int GetHashCode(LogMessage obj)
  {
    var hashCode = new HashCode();
    hashCode.Add((int)obj.Level);
    hashCode.Add(obj.LoggedMessage, StringComparer.CurrentCultureIgnoreCase);
    hashCode.Add(obj.Exception);
    return hashCode.ToHashCode();
  }

  private static bool Equals(Exception? x, Exception? y)
  {
    if (ReferenceEquals(x, y))
      return true;

    if (ReferenceEquals(x, null))
      return false;

    if (ReferenceEquals(y, null))
      return false;

    return x.GetType() == y.GetType() && string.Equals(x.Message, y.Message, StringComparison.CurrentCultureIgnoreCase);
  }
}