using System;
using Microcelium.Testing.Selenium;
using NUnit.Framework;

namespace Microcelium.Testing.NUnit.Selenium
{
  public interface IProvideWebDriverConfigBuilder
  {
    WebDriverConfigBuilder Builder { get; }
  }
}