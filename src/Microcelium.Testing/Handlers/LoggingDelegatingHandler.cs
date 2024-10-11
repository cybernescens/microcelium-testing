using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing.Handlers;

/// <summary>
/// </summary>
public class LoggingDelegatingHandler : DelegatingHandler
{
  private static readonly Regex MessageRegex = new(
    @"(?<key>\w+):\s(?<val>(?(?=['\[])['\[][^'\]]+['\]]|.+?))(?:,\s|$)",
    RegexOptions.Compiled);

  private static readonly Regex HeaderRegex = new(
    @"\r\n{\r\n\s\s(?<head>.+?)\r\n}",
    RegexOptions.Compiled | RegexOptions.Singleline);

  private static readonly Regex NewLineRegex = new(@"\r\n|\r|\n", RegexOptions.Compiled);

  private readonly ILogger log;

  /// <summary>
  ///   Records Request and Response Envelop Details
  /// </summary>
  /// <param name="lf"></param>
  public LoggingDelegatingHandler(ILoggerFactory lf)
  {
    this.log = lf.CreateLogger<LoggingDelegatingHandler>();
  }

  public bool IncludeContents {get; init; } = false;

  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken) =>
    await LogResponseAsync(await base.SendAsync(await LogRequestAsync(request), cancellationToken));

  private async Task<HttpResponseMessage> LogResponseAsync(HttpResponseMessage response) =>
    await TraceMessage(response, r => Clean(r.ToString()), response.Content);

  private async Task<HttpRequestMessage> LogRequestAsync(HttpRequestMessage request) =>
    await TraceMessage(request, r => Clean(r.ToString()), request.Content);

  private string Clean(string r)
  {
    var norm = HeaderRegex.Replace(r, x => " [ " + NewLineRegex.Replace(x.Groups["head"].Value, ";; ") + " ]");
    var match = MessageRegex.Match(norm);

    if (!match.Success)
      log?.LogInformation($"Unable to reformat message\r\n    {r}");

    var msg = new StringBuilder(Environment.NewLine);

    while (match.Success)
    {
      msg.AppendLine($"    {match.Groups["key"].Value,-15}: {match.Groups["val"].Value}");
      match = match.NextMatch();
    }

    return msg.ToString();
  }

  private async Task<T> TraceMessage<T>(T input, Func<T, string> header, HttpContent? content)
  {
    content ??= new StringContent("<No Content>");
    var msg = new StringBuilder($"{typeof(T).Name.Replace("Http", "").Replace("Message", "")}:")
      .AppendLine(header(input));

    if (IncludeContents)
      msg.AppendLine("    " + await content.ReadAsStringAsync());

    log?.LogInformation(msg.ToString());
    return input;
  }
}