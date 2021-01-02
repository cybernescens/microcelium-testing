using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Net.Http;
using Microcelium.Testing.NUnit.AspNetCore.TestServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Microcelium.Testing.AspNetCore.Tests.Handlers
{
  [Parallelizable(ParallelScope.None)]
  internal class RequestInterceptorDelegatingHandlerFixtures : IRequireTestEndpointOverride
  {
    private string content;

    public Task ServerRun(HttpContext context)
      => context.Response.WriteAsync(context.Request.Headers["foo"].Single());

    public TestServer Endpoint { get; set; }

    [SetUp]
    public async Task SetUp()
    {
      using (var tracingHandler = new RequestInterceptorDelegatingHandler(Endpoint.CreateHandler(), x => x.Headers.Add("foo", new[] {"bar"})))
      using (var httpClient = new HttpClient(tracingHandler) {BaseAddress = Endpoint.BaseAddress})
      using (var response = await httpClient.GetAsync(""))
      {
        content = await response.Content.ReadAsStringAsync();
      }
    }

    [Test]
    public void DelegateIsAbleToInterceptTheRequest()
      => content.Should().Be("bar");
  }
}