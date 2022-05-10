using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

public readonly struct LogMessageComparer : IEqualityComparer<LogMessage>
{
  public static readonly LogMessageComparer Default = new();
  public static readonly LogMessageComparer Contains = new((x, y) => x.Contains(y, StringComparison.CurrentCultureIgnoreCase));
  public static readonly LogMessageComparer Start = new((x, y) => x.StartsWith(y, StringComparison.CurrentCultureIgnoreCase));
  public static readonly LogMessageComparer End = new((x, y) => x.EndsWith(y, StringComparison.CurrentCultureIgnoreCase));
  public static readonly LogMessageComparer Regex = new((x, y) => new Regex(y, RegexOptions.IgnoreCase).IsMatch(x));

  private readonly Func<string, string, bool> messageComparer;

  public LogMessageComparer()
  {
    this.messageComparer = (x, y) => string.Equals(x, y, StringComparison.CurrentCultureIgnoreCase);
  }

  public LogMessageComparer(Func<string, string, bool> messageComparer)
  {
    this.messageComparer = messageComparer;
  }

  public bool Equals(LogMessage x, LogMessage y) =>
    x.GetType() == y.GetType() &&
    x.Level == y.Level &&
    messageComparer(x.LoggedMessage, y.LoggedMessage) &&
    Equals(x.Exception, y.Exception);

  public int GetHashCode(LogMessage obj) =>
    HashCode.Combine(
      obj.Level,
      obj.LoggedMessage.GetHashCode(StringComparison.CurrentCultureIgnoreCase),
      obj.Exception);
  
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