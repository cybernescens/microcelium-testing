using System;
using System.Linq;
using Microcelium.Testing.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Microcelium.Testing
{
  /// <summary>
  ///   Logging Test Framework Extensions
  /// </summary>
  public static class LoggingConfigurationExtensions
  {
    /// <summary>
    ///   Configures Serilog to record <see cref="LogMessage" />s and Allow assertions
    /// </summary>
    /// <param name="cfg">the <see cref="LoggerSinkConfiguration" /></param>
    /// <returns></returns>
    public static LoggerConfiguration TestRepository(this LoggerSinkConfiguration cfg)
      => cfg.Sink(new DelegatingSink(LogMessageBuffer.Instance.Add), LogEventLevel.Verbose);

    public static LoggerConfiguration InitializeForMicroceliumTesting(
      this LoggerConfiguration cfg,
      params (string Source, LogEventLevel MinimumLevel)[] minimumLevelOverrides)
    {
      if (!Log.Logger.GetType().Name.Equals("SilentLogger"))
      {
        Log.Logger.Warning(
          "+++++ WARNING LOGGING IS ALREADY INITIALIZED AND "
          + "MAY NOT HAVE BEEN CONFIGURED WITH THE TestRepository SINK +++++");
      }

      /* Need to figure out a way to make these configurable;
            Probably some sort of Hierarchical approach... i.e.
            Env, LocalConfig, Convention... */

      (string Source, LogEventLevel MinimumLevel)[] Default(string source, LogEventLevel minimumLevel = LogEventLevel.Warning)
        => minimumLevelOverrides.Any(x => x.Source.Equals(source, StringComparison.CurrentCultureIgnoreCase))
          ? new (string Source, LogEventLevel MinimumLevel)[0]
          : new[] {(source, minimumLevel)};

      minimumLevelOverrides = minimumLevelOverrides
        .Concat(Default("Castle"))
        .Concat(Default("Microsoft"))
        .Concat(Default("System"))
        .Concat(Default("NHibernate"))
        .ToArray();

      cfg.MinimumLevel.Debug();

      Array.ForEach(
        minimumLevelOverrides,
        mlo => cfg.MinimumLevel.Override(mlo.Source, mlo.MinimumLevel));

      return cfg.Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.TestRepository();
    }
  }
}