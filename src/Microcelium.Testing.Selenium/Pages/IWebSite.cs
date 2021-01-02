using System;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Microcelium.Testing.Selenium.Pages
{
  public interface IWebSite
  {
    void Initialize(IWebDriver driver, IWebDriverConfig baseAddress, ILogger log);
    TPage NavigateToPage<TPage>(string queryString = null) where TPage : IWebPage, IHaveRelativePath, new();
  }

  
}