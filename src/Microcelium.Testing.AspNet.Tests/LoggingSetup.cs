using System.Diagnostics.CodeAnalysis;
using Microcelium.Testing;
using NUnit.Framework;
using Serilog;

[SetUpFixture]
[SuppressMessage("ReSharper", "CheckNamespace")]
public class LoggingSetup : IConfigureLogging
{
  [OneTimeSetUp]
  public void Initialize()
  {
    Log.Logger = new LoggerConfiguration()
      .InitializeForMicroceliumTesting()
      .CreateLogger();
  }
}