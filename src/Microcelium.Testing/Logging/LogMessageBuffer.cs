using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microcelium.Testing.Logging;

/// <summary>
///   Keeps Track of Log Messages in Memory
/// </summary>
public class LogMessageBuffer : IDisposable
{
  private readonly ConcurrentQueue<LogMessage> buffer = new();

  public void Dispose() { buffer.Clear(); }

  internal void Add(LogMessage message) { buffer.Enqueue(message); }

  internal LogMessage[] GetMessages() => buffer.ToArray();
}