using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microcelium.Testing.Handlers;

public class RequestInterceptorDelegatingHandler : DelegatingHandler
{
  private readonly Action<HttpRequestMessage> intercept;

  public RequestInterceptorDelegatingHandler(Action<HttpRequestMessage> intercept) { this.intercept = intercept; }

  public RequestInterceptorDelegatingHandler(HttpMessageHandler innerHandler, Action<HttpRequestMessage> intercept)
    : base(innerHandler)
  {
    this.intercept = intercept;
  }

  protected override Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    intercept(request);
    return base.SendAsync(request, cancellationToken);
  }
}