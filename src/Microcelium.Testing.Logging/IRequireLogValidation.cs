using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.Logging
{
  /// <summary>
  ///   Initialize your fixture with this interface to initialize the infrastructure
  ///   required to capture log information
  /// </summary>
  [RequireLogValidation]
  public interface IRequireLogValidation
  {
    /// <summary>
    /// Context that allows us to make assertions
    /// </summary>
    LogTestContext LogContext { get; set; }
  }

  /// <summary>
  ///   Initializes some infrastructure to capture logging
  /// </summary>
  public class RequireLogValidation : Attribute, ITestAction
  {
    public void BeforeTest(ITest test)
    {
      if (!(test.Fixture is IRequireLogValidation fixture))
        throw new Exception($"Test should implement interface '{typeof(IRequireLogValidation).FullName}'");
      fixture.LogContext = new LogTestContext(test.FullName);
    }

    public void AfterTest(ITest test) { }

    public ActionTargets Targets => ActionTargets.Test;
  }
}