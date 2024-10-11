using System;
using System.Net;

namespace Microcelium.Testing.Selenium;

public class AuthenticationHelper
{
  private static readonly Lazy<AuthenticationHelper> instance;

  static AuthenticationHelper()
  {
    instance = new Lazy<AuthenticationHelper>(() => new AuthenticationHelper());
  }

  public static AuthenticationHelper Instance => instance.Value;

  public CookieContainer AuthCookies { get; } = new();
}