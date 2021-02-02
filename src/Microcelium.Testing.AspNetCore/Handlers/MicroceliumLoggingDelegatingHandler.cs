using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.AspNetCore.Handlers
{
  /// <summary>
  ///   Makes detailed logs of Https Requests
  /// </summary>
  public class MicroceliumLoggingDelegatingHandler : DelegatingHandler
  {
    private static readonly Regex MessageRegex = new Regex(@"(?<key>\w+):\s(?<val>(?(?=['\[])['\[][^'\]]+['\]]|.+?))(?:,\s|$)", RegexOptions.Compiled);
    private static readonly Regex HeaderRegex = new Regex(@"\r\n{\r\n\s\s(?<head>.+?)\r\n}", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex NewLineRegex = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);

    private readonly bool includeContents;
    private readonly ILogger log;

    /// <summary>
    /// Instantiates a <see cref="MicroceliumLoggingDelegatingHandler"/>
    /// </summary>
    /// <param name="innerHandler">the parent <see cref="HttpMessageHandler"/></param>
    /// <param name="log">the <see cref="ILogger"/></param>
    public MicroceliumLoggingDelegatingHandler(HttpMessageHandler innerHandler, ILogger log)
      : this(innerHandler, true, log) { }

    /// <summary>
    /// Instantiates a <see cref="MicroceliumLoggingDelegatingHandler"/>
    /// </summary>
    /// <param name="innerHandler">the next handler</param>
    /// <param name="includeContents">true to log the contents of the response</param>
    /// <param name="log">the <see cref="ILogger"/></param>
    public MicroceliumLoggingDelegatingHandler(HttpMessageHandler innerHandler, bool includeContents, ILogger log)
      : base(innerHandler)
    {
      this.includeContents = includeContents;
      this.log = log;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      => await LogResponseAsync(await base.SendAsync(await LogRequestAsync(request), cancellationToken));

    private async Task<HttpResponseMessage> LogResponseAsync(HttpResponseMessage response)
      => await TraceMessage(response, r => Clean(r.ToString()), response.Content);

    private async Task<HttpRequestMessage> LogRequestAsync(HttpRequestMessage request)
      => await TraceMessage(request, r => Clean(r.ToString()), request.Content);

    private string Clean(string r)
    {
      var norm = HeaderRegex.Replace(r, x => " [ " + NewLineRegex.Replace(x.Groups["head"].Value, ";; ") + " ]");
      var match = MessageRegex.Match(norm);

      if (!match.Success)
        log.LogInformation($"Unable to reformat message\r\n    {r}");

      var msg = new StringBuilder(Environment.NewLine);

      while (match.Success)
      {
        msg.AppendLine($"    {match.Groups["key"].Value,-15}: {match.Groups["val"].Value}");
        match = match.NextMatch();
      }

      return msg.ToString();
    }

    private async Task<T> TraceMessage<T>(T input, Func<T, string> header, HttpContent content)
    {
      content ??= new StringContent("<No Content>");
      var msg = new StringBuilder($"{typeof(T).Name.Replace("Http", "").Replace("Message", "")}:")
        .AppendLine(header(input));

      if (includeContents)
        msg.AppendLine("    " + await content.ReadAsStringAsync());
        
      log.LogInformation(msg.ToString());
      return input;
    }
  }
}