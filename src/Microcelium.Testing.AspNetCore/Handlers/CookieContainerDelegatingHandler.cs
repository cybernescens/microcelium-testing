using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microcelium.Testing.AspNetCore.Handlers
{
  /// <summary>
  /// A <see cref="DelegatingHandler"/> to facilitate Cookie testing
  /// </summary>
  public class CookieContainerDelegatingHandler : DelegatingHandler
  {
    private readonly CookieContainer container;

    /// <summary>
    /// Instantiates a <see cref="CookieContainerDelegatingHandler"/>
    /// </summary>
    /// <param name="container">the <see cref="CookieContainer"/></param>
    /// <param name="handler">the parent <see cref="HttpMessageHandler"/></param>
    public CookieContainerDelegatingHandler(CookieContainer container, HttpMessageHandler handler) : base(handler)
    {
      this.container = container;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      request.Headers.Add("Cookie", container.GetCookieHeader(request.RequestUri));

      ApplyCookies(request);
      var response = await base.SendAsync(request, cancellationToken);
      SetCookies(response);
      return response;
    }

    private void SetCookies(HttpResponseMessage response, Uri requestUri = null)
    {
      if (!response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
        return;

      foreach (var cookie in cookieHeaders)
      {
        container.SetCookies(requestUri ?? response.RequestMessage.RequestUri, cookie);
      }
    }

    private void ApplyCookies(HttpRequestMessage request)
    {
      var cookieHeader = container.GetCookieHeader(request.RequestUri);

      if (!string.IsNullOrEmpty(cookieHeader))
      {
        request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
      }
    }
  }
}