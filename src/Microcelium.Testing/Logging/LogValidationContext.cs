using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Microcelium.Testing.Logging;

/// <summary>
///   Wrapper object to help facilitate assertions
/// </summary>
public class LogValidationContext : IDisposable
{
  private readonly LogMessageBuffer buffer;
  private volatile int replayCutoff = 20;

  public LogValidationContext(LogMessageBuffer buffer)
  {
    this.buffer = buffer;
  }

  /// <summary>
  ///   Total number of messages displayed in Assertion Failures
  /// </summary>
  public int ReplayCutoff => replayCutoff;

  /// <summary>
  ///   Sets the total number of messages to be displayed in Assertion Failures
  /// </summary>
  /// <param name="cutoff">the number of displayed messages</param>
  /// <returns>a reference to itself</returns>
  public LogValidationContext SetReplayCutoff(int cutoff)
  {
    replayCutoff = cutoff;
    return this;
  }

  /// <summary>
  /// Finds all matching log messages
  /// </summary>
  /// <param name="expectation">the <see cref="LogMessage"/> expectation</param>
  /// <param name="comparer">the <see cref="LogMessageComparer"/></param>
  /// <returns>all matching <see cref="LogMessage"/>s</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public IEnumerable<LogMessage> FindMatchingLogMessages(
    LogMessage expectation,
    LogMessageComparer comparer = default) =>
    buffer.GetMessages().Where(x => comparer.Equals(x, expectation));

  /// <summary>
  /// Finds all matching log messages by building a <see cref="LogMessage"/> expectation
  /// </summary>
  /// <param name="message">the message text</param>
  /// <param name="level">the <see cref="LogLevel"/></param>
  /// <param name="mode">the <see cref="MatchMode"/> for evaluating the <see cref="LogMessage.LoggedMessage"/></param>
  /// <param name="exception">the <see cref="Exception"/> when matching against <see cref="LogMessage.Exception"/></param>
  /// <returns>all matching <see cref="LogMessage"/>s</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public IEnumerable<LogMessage> FindMatchingLogMessages(
    string message,
    LogLevel level = LogLevel.Information,
    MatchMode mode = MatchMode.Contains,
    Exception? exception = null)
  {
    var (comparer, _) = Predicate(mode);
    return FindMatchingLogMessages(new LogMessage(level, message, exception), comparer);
  }

  /// <summary>
  /// Asserts the logger received the expected <see cref="LogMessage"/>
  /// </summary>
  /// <param name="expectation">the <see cref="LogMessage"/> expectation</param>
  /// <param name="comparer">the <see cref="LogMessageComparer"/></param>
  /// <param name="errorMsg">the error message set with the assertion failure</param>
  /// <returns>a reference to itself or an <see cref="LogMessageAssertionFailedException"/></returns>
  /// <exception cref="LogMessageAssertionFailedException"></exception>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public LogValidationContext ReceivedExpectation(
    LogMessage expectation,
    LogMessageComparer comparer = default,
    string errorMsg = "Did not find a log message matching")
  {
    var messages = buffer.GetMessages();
    if (messages.Any(x => comparer.Equals(x, expectation)))
      return this;

    throw new LogMessageAssertionFailedException(errorMsg, expectation, messages, ReplayCutoff);
  }

  /// <summary>
  /// Builds a <see cref="LogMessage"/> expectation and asserts the logger received it
  /// </summary>
  /// <param name="message">the message text</param>
  /// <param name="level">the <see cref="LogLevel"/></param>
  /// <param name="mode">the <see cref="MatchMode"/> for evaluating the <see cref="LogMessage.LoggedMessage"/></param>
  /// <param name="exception">the <see cref="Exception"/> when matching against <see cref="LogMessage.Exception"/></param>
  /// <returns>a reference to itself or an <see cref="LogMessageAssertionFailedException"/></returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public LogValidationContext Received(
    string message,
    LogLevel level = LogLevel.Information,
    MatchMode mode = MatchMode.Contains,
    Exception? exception = null)
  {
    var (comparer, errorMsg) = Predicate(mode);
    ReceivedExpectation(new LogMessage(level, message, exception), comparer, errorMsg);
    return this;
  }

  private static (LogMessageComparer Func, string Msg) Predicate(MatchMode mm) =>
    mm switch {
      MatchMode.Contains => (LogMessageComparer.Contains, "Did not find a message containing"),
      MatchMode.Start => (LogMessageComparer.Start, "Did not find a message starting with"),
      MatchMode.End => (LogMessageComparer.End, "Did not find a message ending with"),
      MatchMode.Regex => (LogMessageComparer.Regex, "Did not find a message matching pattern"),
      _ => (LogMessageComparer.Default, "Did not find a message matching")
    };

  /// <inheritdoc />
  public void Dispose()
  {
    buffer.Dispose();
  }

  public void Log<TState>(
    string name,
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
  {
    // should do a scope for eventId right here 

    buffer.Add(
      new LogMessage(
        logLevel,
        formatter(state, exception),
        exception,
        DateTime.Now,
        name,
        scopes.SelectMany(x => x).ToArray()));
  }

  public IDisposable BeginScope<TState>(TState state)
  {
    var factory = ScopeStateFactory(state);
    var properties = new ScopedState();

    try
    {
      properties = factory(state);
    }
    catch { /* intentional swallow */ }

    scopes.Push(properties);
    return new DelegatedScope(() => { PopScopeState(properties); });
  }

  private readonly ConcurrentStack<ScopedState> scopes = new();

  private void PopScopeState(ScopedState state)
  {
    /* note, I'm thinking this will probably cause deadlock or stall forever...
        I can't see it happening, but it smells; only time it would fail 
        is when a scope is not properly disposed.
    
     Hopefully the calls to Volatile help with this */

    var stop = false;

    var timer = new Timer(1000) { AutoReset = false };
    timer.Elapsed += (_, _) => Volatile.Write(ref stop, true);
    timer.Start();

    for (; !scopes.TryPeek(out var s);)
      if (!Volatile.Read(ref stop) && s.Equals(state))
        return;

    throw new InvalidOperationException("Attempted to dispose a state that no longer exists");
  }

  [DebuggerDisplay("{display},nq")]
  private struct ScopedState : IEnumerable<(string Key, string Value)>, IEnumerator<(string Key, string Value)>
  {
    private readonly string[] keys;
    private readonly string[] values;
    private readonly int length = 0;
    private readonly string display;

    private int position = -1;

    public ScopedState() : this(Array.Empty<string>(), Array.Empty<string>()) { }

    public ScopedState(string[] keys, string[] values)
    {
      if (keys.Length != values.Length)
        throw new ArgumentException(
          $"{nameof(keys)} (Length: {keys.Length}) and {nameof(values)} (Length: {values.Length}) must have identical lengths");

      this.keys = keys;
      this.values = values;
      length = keys.Length;

      var s = new StringBuilder();
      for (var i = 0; i < length; i++)
      {
        s.Append($"{keys[i]}:{values[i]}");
        if (i + 1 != length)
          s.Append("; ");
      }

      display = s.ToString();
    }

    public IEnumerator<(string Key, string Value)> GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => this;

    public bool MoveNext() => ++position < length;
    public void Reset() { position = -1; }

    public (string Key, string Value) Current => (keys[position], values[position]);
    object IEnumerator.Current => (keys[position], values[position]);

    public override string ToString() => display;

    public void Dispose() { }
  }

  /* few things to do here and it's pretty optional really...
      if we are a dictionary, or name value collection or some sort of similar key-value
      pair type, then we just want to iterate that object so we can easily report on
      those key-value pairs.

      if we are not a primitive type and are an object or struct or similar thing
      with public properties or fields, then we just want to iterate that object
      as if it were a key-value pair as well.
   */
  private static readonly Type GenericDictionary = typeof(IDictionary<,>);
  private static readonly Type GenericKeyValuePair = typeof(KeyValuePair<,>);
  private static readonly PropertyInfo KvpKeyProperty = GenericKeyValuePair.GetProperty("Key")!;
  private static readonly PropertyInfo KvpValueProperty = GenericKeyValuePair.GetProperty("Value")!;
  private static readonly Type Dictionary = typeof(IDictionary);

  private static Func<T, ScopedState> ScopeStateFactory<T>(T t)
  {
    if (t == null)
      return NoopProperties;

    var stateType = typeof(T);
    var interfaces = stateType.GetInterfaces();
    if (interfaces.Where(x => x.IsGenericType)
        .Select(x => (Type: x.GetGenericTypeDefinition(), Args: x.GetGenericArguments()))
        .Any(x => GenericDictionary.IsAssignableFrom(x.Type)))
      return IterateGenericStringKeyedDictionary;

    if (interfaces.Any(x => Dictionary.IsAssignableFrom(x)))
      return IterateDictionary;

    return IteratePublicPropertiesAndFields;
  }

  private static ScopedState NoopProperties<T>(T state) => new();

  private static ScopedState IterateGenericStringKeyedDictionary<T>(T state)
  {
    var keys = new List<string>();
    var values = new List<string>();
    var list = (IEnumerable)state!;
    var e = list.GetEnumerator();

    for (var i = 0; e.MoveNext(); i++)
    {
      keys.Add(KvpKeyProperty.GetValue(e.Current)?.ToString() ?? i.ToString());
      values.Add(KvpValueProperty.GetValue(e.Current)?.ToString() ?? string.Empty);
    }

    return new ScopedState(keys.ToArray(), values.ToArray());
  }

  private static ScopedState IterateDictionary<T>(T state)
  {
    var dict = (IDictionary)state!;

    if (dict.Count < 1)
      return new ScopedState(Array.Empty<string>(), Array.Empty<string>());

    var keys = new string[dict.Count];
    var values = new string[dict.Count];
    var e = dict.GetEnumerator();

    for (var i = 0; e.MoveNext(); i++)
    {
      keys[i] = e.Key?.ToString() ?? i.ToString();
      values[i] = e.Value?.ToString() ?? string.Empty;
    }

    return new ScopedState(keys, values);
  }

  private static ScopedState IteratePublicPropertiesAndFields<T>(T state)
  {
    var stateType = typeof(T);
    var properties = stateType.GetProperties();
    var fields = stateType.GetFields();

    var length = properties.Length + fields.Length;
    var keys = new string[length];
    var values = new string[length];
    var offset = properties.Length;

    for (var i = 0; i < length; i++)
    {
      keys[i] = properties[i].Name;
      values[i] = properties[i].GetValue(state)?.ToString() ?? string.Empty;
    }

    for (var i = 0; i < length; i++)
    {
      keys[offset + i] = fields[i].Name;
      values[offset + i] = fields[i].GetValue(state)?.ToString() ?? string.Empty;
    }

    return new ScopedState(keys, values);
  }

  private class DelegatedScope : IDisposable
  {
    private readonly Action callback;
    public DelegatedScope(Action callback) { this.callback = callback; }
    public void Dispose() { SafelyTry.Action(() => callback()); }
  }
}