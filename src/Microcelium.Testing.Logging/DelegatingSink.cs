using System;
using Serilog.Core;
using Serilog.Events;

namespace Microcelium.Testing.Logging
{
  /// <summary>
  ///   Configures a sink allowing us to make assertions on received log messages
  /// </summary>
  public class DelegatingSink : ILogEventSink
  {
    private readonly Action<LogEvent> write;

    public DelegatingSink(Action<LogEvent> write)
    {
      this.write = write ?? throw new ArgumentNullException(nameof(write));
    }

    public void Emit(LogEvent logEvent) => write(logEvent);
  }
}