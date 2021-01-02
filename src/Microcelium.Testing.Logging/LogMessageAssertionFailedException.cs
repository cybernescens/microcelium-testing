using System;
using System.Linq;
using System.Text;

namespace Microcelium.Testing.Logging
{
  /// <summary>
  ///   Assertion Exception when LogMessages to not meet our expectations
  /// </summary>
  public class LogMessageAssertionFailedException : AssertionException
  {
    //private const string IgnorableNamespace = "Microcelium.Testing.Logging";
    public LogMessageAssertionFailedException(string message, LogMessage expectation, LogMessage[] buffer, int cutoff = 20)
    {
      var seed = "+++ Received No Log Messages +++";
      if (buffer.Length > cutoff)
        seed = $"+++ Not going to display; more than {cutoff} log messages received. +++{Environment.NewLine}\t";

      var replay = buffer
        .Where((lm, i) => i < cutoff)
        .Aggregate(
          new StringBuilder(seed),
          (sb, lm) => sb.Append($"\t{Environment.NewLine}{lm.LoggedMessage}"),
          sb => sb.ToString());

      Message = $"{message}: {Environment.NewLine}"
        + $"\t{expectation}{Environment.NewLine}"
        + $"Received these log messages: {Environment.NewLine}"
        + $"\t{replay}";
    }

    public override string Message { get; }
  }
}