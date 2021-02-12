using System;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing.NUnit
{
  public class TraceTestNameAttribute : Attribute, ITestAction, IRequireLogger
  {
    private readonly ILogger log;

    public TraceTestNameAttribute() { log = this.CreateLogger(); }

    public ActionTargets Targets => ActionTargets.Test;

    public void BeforeTest(ITest testDetails) { log.LogInformation($"Before: {testDetails.FullName}"); }

    public void AfterTest(ITest testDetails) => log.LogInformation($"After: {testDetails.FullName}");
  }
}