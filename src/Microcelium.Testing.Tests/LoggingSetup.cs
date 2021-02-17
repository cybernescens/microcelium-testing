using Microcelium.Testing;
using NUnit.Framework;
using Serilog;

[SetUpFixture]
public class LoggingSetup
{
  [OneTimeSetUp]
  public void Initialize()
  {
    Log.Logger = new LoggerConfiguration()
      .InitializeForMicroceliumTesting()
      .CreateLogger();
  }
}