using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Serilog.Events;

namespace Microcelium.Testing.Logging
{
  /// <summary>
  ///   Keeps Track of Log Messages in Memory
  /// </summary>
  internal class LogMessageBuffer
  {
    public const string TestContext = "MicroceliumTestName";
    private static readonly Regex StripQuotes = new Regex("\"", RegexOptions.Compiled);

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static readonly Lazy<LogMessageBuffer> instance
      = new Lazy<LogMessageBuffer>(() => new LogMessageBuffer());

    private readonly ConcurrentDictionary<string, ConcurrentQueue<LogMessage>> buffer;

    private LogMessageBuffer()
    {
      buffer = new ConcurrentDictionary<string, ConcurrentQueue<LogMessage>>();
    }

    internal static LogMessageBuffer Instance => instance.Value;

    internal void Add(LogEvent le)
    {
      le.Properties.TryGetValue(TestContext, out var ctx);
      var key = ctx?.ToString() ?? typeof(UnknownTest).FullName;
      key = StripQuotes.Replace(key, string.Empty);
      var queue = buffer.GetOrAdd(key, new ConcurrentQueue<LogMessage>());
      queue.Enqueue(new LogMessage(le));
    }

    internal LogMessage[] GetMessages(LogTestContext ctx)
      => buffer.TryGetValue(ctx.Name, out var queue)
        ? queue.ToArray()
        : buffer.TryGetValue(typeof(UnknownTest).FullName, out queue)
          ? queue.ToArray()
          : new LogMessage[0];
  }

  /// <summary>
  ///   When LogMessages are missing the required "MicroceliumTestName" property then we will
  ///   use this as a place holder, no matter what test it came from.
  /// </summary>
  internal class UnknownTest { }
}