using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microcelium.Testing.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace Microcelium.Testing.Handlers;

[Parallelizable(ParallelScope.None)]
[RequireWebEndpoint]
internal class RequestInterceptorDelegatingHandlerFixtures : IRequireWebHostOverride
{
  private string content;

  public void Configure(WebApplication endpoint)
  {
    endpoint.Run(
      context => context.Response.WriteAsync(context.Request.Headers["foo"].Single()));
  }

  [SetUp]
  public async Task SetUp()
  {
    var name = "require-interceptor";

    var sc = new ServiceCollection();
    sc.AddHttpClient(name, client => client.BaseAddress = HostUri)
      .AddHttpMessageHandler(_ => new RequestInterceptorDelegatingHandler(x => x.Headers.Add("foo", new[] { "bar" })));
    
    var factory = sc.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

    using (var httpClient = factory.CreateClient(name))
    using (var response = await httpClient.GetAsync(""))
    {
      content = await response.Content.ReadAsStringAsync();
    }
  }

  [Test]
  public void DelegateIsAbleToInterceptTheRequest() => content.Should().Be("bar");

  public IHost Host { get; set; }
  public WebApplication Endpoint { get; set; }
  public Uri HostUri { get; set; }
}