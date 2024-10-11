using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microcelium.Testing;

public interface IRequireLogging : IRequireHost
{
  ILoggerFactory LoggerFactory { get; set; }
}

public interface IConfigureLogging
{
  void Apply(HostBuilderContext context, ILoggingBuilder logging);
}