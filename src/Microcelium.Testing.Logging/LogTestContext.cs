using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Logging
{
  /// <summary>
  ///   Wrapper object to help facilitate assertions
  /// </summary>
  public class LogTestContext
  {
    private volatile int replayCutoff = 20;

    internal LogTestContext(string name)
    {
      Name = name;
    }

    /// <summary>
    /// Name of the Current Test Logging Context
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Total number of messages displayed in Assertion Failures
    /// </summary>
    public int ReplayCutoff => replayCutoff;

    /// <summary>
    /// Sets the total number of messages to be displayed in Assertion Failures
    /// </summary>
    /// <param name="cutoff">the number of displayed messages</param>
    /// <returns>a reference to itself</returns>
    public LogTestContext SetReplayCutoff(int cutoff)
    {
      replayCutoff = cutoff;
      return this;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public LogTestContext ReceivedExpectation(Func<LogMessage> expectationBuilder, Func<LogMessage, LogMessage, bool> match, string errorMsg = "Did not find a log message matching")
    {
      var buffer = LogMessageBuffer.Instance;
      var messages = buffer.GetMessages(this);
      var expectation = expectationBuilder();
      if (messages.Any(x => match(x, expectation)))
        return this;

      throw new LogMessageAssertionFailedException(errorMsg, expectation, messages, ReplayCutoff);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public LogTestContext Received(string message, LogLevel level = LogLevel.Information, MatchMode mode = MatchMode.Contains, Exception exception = null)
    {
      (Func<LogMessage, LogMessage, bool> Func, string Msg) Predicate()
      {
        return mode switch {
          MatchMode.Contains => (
            (actual, expect) =>
              actual.Equals(expect, (msga, msge) => msga.ToLower().Contains(msge?.ToLower() ?? string.Empty)),
            "Did not find a message containing"),
          MatchMode.Start => (
            (actual, expect) => actual.Equals(expect,
              (msga, msge) => msga.StartsWith(msge, StringComparison.CurrentCultureIgnoreCase)),
            "Did not find a message starting with"),
          MatchMode.End => (
            (actual, expect) => actual.Equals(expect,
              (msga, msge) => msga.EndsWith(msge, StringComparison.CurrentCultureIgnoreCase)),
            "Did not find a message ending with"),
          MatchMode.Regex => (
            (actual, expect) => actual.Equals(expect,
              (msga, msge) => new Regex(message, RegexOptions.IgnoreCase).IsMatch(msga)),
            $"Did not find a message matching pattern '{message}'"),
          _ => (
            (actual, expect) => actual.Equals(expect,
              (msga, msge) => msga.Equals(msge, StringComparison.CurrentCultureIgnoreCase)),
            "Did not find a message matching")
        };
      }

      var predicate = Predicate();
      ReceivedExpectation(() => new LogMessage(level, message, exception), predicate.Func, predicate.Msg);
      return this;
    }
  }
}