using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Microcelium.Testing.Logging
{
  public sealed class LogMessage : IEquatable<LogMessage>
  {
    private static readonly Regex CleanQuotes = new Regex("\"", RegexOptions.Compiled);
    private const string SourceContext = "SourceContext";
    private readonly DateTime now;

    internal LogMessage(LogLevel level, string message, Exception exception = null)
    {
      Name = "+++++ expectation +++++";
      Level = level;
      Exception = exception;
      LoggedMessage = message;
      now = DateTime.Now;
    }

    internal LogMessage(LogEvent le)
    {
      LogLevel Level()
      {
        switch (le.Level)
        {
          case LogEventLevel.Verbose:
          case LogEventLevel.Debug: return LogLevel.Debug;
          case LogEventLevel.Information: return LogLevel.Information;
          case LogEventLevel.Warning: return LogLevel.Warning;
          case LogEventLevel.Error: return LogLevel.Error;
          case LogEventLevel.Fatal: return LogLevel.Critical;
          default: return LogLevel.Information;
        }
      }

      this.Level = Level();
      now = le.Timestamp.DateTime;
      Name = CleanQuotes.Replace(le.Properties[SourceContext].ToString(), string.Empty);
      LoggedMessage = le.RenderMessage(CultureInfo.CurrentCulture);
      Exception = le.Exception;
      Properties = le.Properties.ToLookup(x => x.Key, x => x.Value.ToString());
    }

    public string Name { get; }
    public LogLevel Level { get; }
    public string LoggedMessage { get; }
    public Exception Exception { get; }
    public ILookup<string, string> Properties { get; set; }

    public bool Equals(LogMessage other, Func<string, string, bool> predicate)
      => !ReferenceEquals(null, other)
        && (ReferenceEquals(this, other)
          || Level == other.Level
          && predicate(LoggedMessage, other.LoggedMessage)
          && Equals(other?.Exception));

    public bool Equals(LogMessage other) => Equals(other, string.Equals);

    public override bool Equals(object obj) => !ReferenceEquals(null, obj)
      && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((LogMessage)obj));

    private bool Equals(Exception other)
      => (ReferenceEquals(null, Exception) && ReferenceEquals(null, other)) 
          || (ReferenceEquals(Exception, other)) 
          || (!ReferenceEquals(null, Exception) && !ReferenceEquals(null, other)
                && Exception.GetType() == other.GetType()
                && Exception.Message.Equals(other.Message, StringComparison.CurrentCultureIgnoreCase));

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (int)Level;
        hashCode = (hashCode * 397) ^ (LoggedMessage?.GetHashCode() ?? 0);
        hashCode = (hashCode * 397) ^ (Exception?.GetHashCode() ?? 0);
        return hashCode;
      }
    }

    public override string ToString()
    {
      var message = LoggedMessage;
      if (Exception != null)
        message = message + "|" + Exception;

      return $"{now} | {Level} | {Name} | {message}";
    }
  }
}