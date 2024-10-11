using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microcelium.Testing.Handlers;

public class CookieContainerDelegatingHandler : DelegatingHandler
{
  private readonly CookieContainer container;

  public CookieContainerDelegatingHandler(CookieContainer container, HttpMessageHandler handler) : base(handler)
  {
    this.container = container;
  }

  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    if (request.RequestUri != null)
      request.Headers.Add("Cookie", container.GetCookieHeader(request.RequestUri));

    ApplyCookies(request);
    var response = await base.SendAsync(request, cancellationToken);
    SetCookies(response);
    return response;
  }

  private void SetCookies(HttpResponseMessage response, Uri? requestUri = null)
  {
    if (!response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
      return;

    if (requestUri == null && response.RequestMessage?.RequestUri == null)
      return;

    foreach (var cookie in cookieHeaders)
      container.SetCookies(requestUri ?? response.RequestMessage?.RequestUri!, cookie);
  }

  private void ApplyCookies(HttpRequestMessage request)
  {
    if (request.RequestUri != null)
    {
      var cookieHeader = container.GetCookieHeader(request.RequestUri);

      if (!string.IsNullOrEmpty(cookieHeader))
        request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
    }
  }
}