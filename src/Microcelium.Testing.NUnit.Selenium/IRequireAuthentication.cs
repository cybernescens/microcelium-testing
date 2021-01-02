namespace Microcelium.Testing.NUnit.Selenium
{
  public interface IRequireAuthentication
  {
    IAuthenticationHelper AuthenticationHelper { get; set; }
  }
}