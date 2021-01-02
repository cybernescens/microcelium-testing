using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microcelium.Testing
{
  public class AssertionException : Exception
  {
    private static readonly string[] IgnoreNamespaces =
        {"Microcelium.Testing.AssertionException", "Microcelium.Testing.Exception", "Microcelium.Testing.Logging", "NUnit"};

    private static readonly MethodInfo MethodInfoAppend
      = typeof(ResolvedMethod).GetMethod("Append", BindingFlags.Instance | BindingFlags.NonPublic);

    public AssertionException()
    {
      StackTrace = PrepareStackTrace();
    }

    public override string StackTrace { get; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string PrepareStackTrace()
    {
      var frames = EnhancedStackTrace.Current().ToList();
      var sb = new StringBuilder();
      var done = false;

      for (var fi = 0; !(sb.Length > 0 && done); fi++)
      {
        var frame = frames[fi];
        var methodTypeName = frame.MethodInfo.DeclaringType.FullName;
        if (IgnoreNamespaces.All(x => !methodTypeName.StartsWith(x)))
        {
          if (sb.Length > 0)
            sb.Append(Environment.NewLine);

          sb.Append("   at ");
          MethodInfoAppend.Invoke(frame.MethodInfo, new[] {(object)sb});

          var filePath = frame.GetFileName();
          if (!string.IsNullOrEmpty(filePath))
          {
            sb.Append(" in ");
            sb.Append(EnhancedStackTrace.TryGetFullPath(filePath));
          }

          var lineNo = frame.GetFileLineNumber();
          if (lineNo != 0)
          {
            sb.Append(":line ");
            sb.Append(lineNo);
          }

          continue;
        }

        done = sb.Length > 0;
      }

      return sb.ToString();
    }
  }
}