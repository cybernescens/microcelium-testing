using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium
{
  public class Javascript
  {
    private readonly string function;
    private readonly ILogger log;

    private Javascript(string function, ILogger log)
    {
      this.function = function;
      this.log = log;
    }

    public static Javascript FunctionResult(string function, ILogger log) => new Javascript(function, log);

    public Func<IWebDriver, bool> Matches<TExpectedResult>(TExpectedResult expectedResult, bool matches = true)
      => driver =>
        {
          bool result;
          try
          {
            log.LogInformation("Waiting for '{0}' to equal '{1}'...", function, expectedResult);
            var javascriptResult = driver.ExecuteScript<TExpectedResult>($"return {function}");
            result = !(EqualityComparer<TExpectedResult>.Default.Equals(javascriptResult, expectedResult) ^ matches); //!xor => xand
            log.LogInformation("{0} = '{1}'", function, javascriptResult);
          }
          catch (Exception e)
          {
            log.LogError(e, "Error executing javascript.");
            result = true;
          }

          return result;
        };
  }
}