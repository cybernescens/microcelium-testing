using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium.WebDriverExtensionsFixtures
{
  [Parallelizable(ParallelScope.Fixtures)]
  internal class WaitingForJavascriptResultToMatch : IRequireLogger 
  { 
    [Test]
    public async Task ReturnsTrueForExpectedMatch()
    {
      var log = this.CreateLogger();
      var args = new NameValueCollection();
      var url = $"http://localhost:{TcpPort.NextFreePort()}";
      args.Add("selenium.baseUrl", url);

      var browserConfig = WebDriver
        .Configure(cfg => cfg.WithDefaultOptions().Providers(x => args[x]), log)
        .Build();

      using (WebHost.Start(
        url,
        router => router
          .MapGet(
            "",
            (req, res, data) =>
              {
                res.ContentType = "text/html";
                return res.WriteAsync("<body></body>");
              })))
      using (var driver = WebDriverFactory.Create(browserConfig))
      {
        driver.Navigate().GoToUrl(url);
        driver.WaitForJavascriptResult("6 / 3", 2, log, TimeSpan.FromMilliseconds(100));
      }
    }
  }
}