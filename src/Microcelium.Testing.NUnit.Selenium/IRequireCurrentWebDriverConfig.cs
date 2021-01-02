using Microcelium.Testing.Selenium;

namespace Microcelium.Testing.NUnit.Selenium
{
  public interface IRequireCurrentWebDriverConfig
  {
    IWebDriverConfig Config { get; set; }
  }
}