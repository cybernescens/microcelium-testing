using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microcelium.Testing.Net;
using Microcelium.Testing.NUnit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Microcelium.Testing.Selenium.WebDriverExtensionsFixtures
{
  [Parallelizable(ParallelScope.Fixtures)]
  internal class WaitingForJavascriptResultToMatch : IRequireLogger 
  { 
    [Test]
    public void ReturnsTrueForExpectedMatch()
    {
      var log = this.CreateLogger();
      var services = new ServiceCollection();
      var args = new NameValueCollection();
      var url = $"http://localhost:{TcpPort.NextFreePort()}";
      args.Add("BaseUrl", url);

      services.AddInMemoryWebDriverConfig(args.Keys.Cast<string>().Select(x => KeyValuePair.Create(x, args[x])));
      var sp = services.BuildServiceProvider();
      var browserConfig = sp.GetRequiredService<IOptions<WebDriverConfig>>().Value;

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