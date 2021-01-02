namespace Microcelium.Testing.Logging
{
  /// <summary>
  /// An enumeration of Match Modes possible between actual and expected log messages
  /// All checks are case-insensitive
  /// </summary>
  public enum MatchMode
  {
    /// <summary>
    /// The actual message is an exact match to the expected message
    /// </summary>
    Exact,

    /// <summary>
    /// The actual message contains the expected message
    /// </summary>
    Contains,

    /// <summary>
    /// The actual message starts with the expected message
    /// </summary>
    Start,

    /// <summary>
    /// The actual message ends with the expected message
    /// </summary>
    End,

    /// <summary>
    /// The actual message matches the provided regex in the expected message
    /// </summary>
    Regex
  }
}