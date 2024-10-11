using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium;

public class Javascript
{
  private readonly string function;
  private readonly ILogger<Javascript> log;

  private Javascript(string function, ILoggerFactory lf)
  {
    this.function = function;
    log = lf.CreateLogger<Javascript>();
  }

  public static Javascript FunctionResult(string function, ILoggerFactory lf) => new(function, lf);

  public Func<IWebDriver, bool> DoesNotMatch<TExpectedResult>(TExpectedResult expectation)
    where TExpectedResult : IConvertible =>
    d => Evaluate(d, expectation, false);

  public Func<IWebDriver, bool> Matches<TExpectedResult>(TExpectedResult expectation)
    where TExpectedResult : IConvertible =>
    d => Evaluate(d, expectation, true);

  private bool Evaluate<TExpectedResult>(
    IWebDriver driver,
    TExpectedResult expectation,
    bool matches) where TExpectedResult : IConvertible
  {
    TExpectedResult? ExecuteScript(string script) 
    {
      var js = (IJavaScriptExecutor)driver;
      var executeScript = js.ExecuteScript(script);
      if (executeScript == null)
        return default;

      return (TExpectedResult)Convert.ChangeType(executeScript, typeof(TExpectedResult));
    }

    try
    {
      log.LogInformation("Waiting for '{Function}' to equal '{Result}'...", function, expectation);
      var javascriptResult = ExecuteScript($"return {function}");
      log.LogInformation("{Function} = '{Result}'", function, javascriptResult);

      //!xor => xand
      return !(EqualityComparer<TExpectedResult>.Default.Equals(javascriptResult, expectation) ^ matches);
    }
    catch (Exception e)
    {
      log.LogError(e, "Error executing javascript.");
    }

    return !matches;
  }
}