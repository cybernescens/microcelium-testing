using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.AspNetCore.Handlers;
using Microcelium.Testing.NUnit.AspNetCore.TestServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Microcelium.Testing.AspNetCore.Tests.Handlers
{
  [Parallelizable(ParallelScope.None)]
  class CookieContainerDelegatingHandlerFixtures : IRequireTestEndpointOverride
  {
    private CookieContainer container;
    private (string key, string value)[] receivedCookies;

    [SetUp]
    public async Task SetUp()
    {
      container = new CookieContainer();
      container.Add(new Cookie("test1", "A", "/", "sitewithcookies"));
      container.Add(new Cookie("test2", "B", "/", "sitewithcookies"));
      container.Add(new Cookie("X", "Y", "/", "someothersite"));
      using (var tracingHandler = new CookieContainerDelegatingHandler(container, Endpoint.CreateHandler()))
      using (var httpClient = new HttpClient(tracingHandler) { BaseAddress = new Uri("http://sitewithcookies") })
      using (var response = await httpClient.GetAsync(""))
      {
        await response.Content.ReadAsStringAsync();
      }
    }

    [Test]
    public void ServerReceivedCookiesFromContainer() =>
      receivedCookies.Should().Equal(
        ("test1", "A"), ("test2", "B"));

    [Test]
    public void CookieContainerContainsReturnedCookiesAndOriginalCookies()
      => container
        .GetCookies(new Uri("http://sitewithcookies"))
        .Cast<Cookie>()
        .Should().Equal(
          new Cookie("foo", "bar", "/", "sitewithcookies"),
          new Cookie("wibble", "wobble", "/", "sitewithcookies"),
          new Cookie("test1", "A", "/", "sitewithcookies"),
          new Cookie("test2", "B", "/", "sitewithcookies"));

    public Task ServerRun(HttpContext context)
    {
      receivedCookies = context.Request.Cookies.Select(x => (x.Key, x.Value)).ToArray();
      context.Response.Cookies.Append("foo", "bar");
      context.Response.Cookies.Append("wibble", "wobble");
      return context.Response.WriteAsync("hello world");
    }

    public TestServer Endpoint { get; set; }
  }
}