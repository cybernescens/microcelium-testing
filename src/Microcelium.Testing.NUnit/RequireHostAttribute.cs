using System;
using System.Reflection;
using System.Threading.Tasks;
using Microcelium.Testing.Logging;
using Microcelium.Testing.Specs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Microcelium.Testing;

public abstract class RequireHostAttribute : TestActionAttribute 
{
  private static readonly Type SpecsType = typeof(SpecsFor<,>);
  private static readonly Type AsyncSpecsType = typeof(AsyncSpecsFor<,>);

  public IHost Host { get; set; } = null!;
  public ILoggerFactory LoggerFactory { get; set; } = null!;

  //protected IServiceScope? serviceScope;
  protected LogValidationContext? logContext;
  private bool requireLogValidation;
  private IConfiguration? configuration;

  protected abstract IRequireHost Fixture { get; }

  protected T? GetSetting<T>(string key)
  {
    if (configuration == null)
      throw new InvalidOperationException("configuration is not yet available");

    key = key.StartsWith("microceliumTests:", StringComparison.Ordinal) ? key : $"microceliumTests:{key}";
    return configuration.GetValue<T>(key);
  }

  /// <summary>
  /// This is always called first to ensure we have a matching Attribute &lt;-&gt; interface pair
  /// </summary>
  /// <param name="test">the executing test</param>
  protected abstract void EnsureFixture(ITest test);

  /// <summary>
  /// Factory method to create the <see cref="IHostBuilder"/>
  /// </summary>
  /// <returns></returns>
  protected abstract IHostBuilder CreateHostBuilder();

  /// <summary>
  /// Factory method to create the <see cref="IHost"/>
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  protected abstract IHost CreateHost(IHostBuilder builder);

  /// <summary>
  /// Runs after <see cref="EnsureFixture(ITest)"/> and before <see cref="CreateHostBuilder"/>
  /// </summary>
  /// <param name="test"></param>
  protected virtual void OnStartBeforeTest(ITest test) { }

  /// <summary>
  /// Runs after the builder has been configured by the base class and before <see cref="CreateHost(IHostBuilder)"/>
  /// </summary>
  /// <param name="builder"></param>
  /// <param name="test"></param>
  protected virtual void OnBeforeCreateHost(IHostBuilder builder, ITest test) { }

  /// <summary>
  /// Runs after <see cref="CreateHostBuilder"/>
  /// </summary>
  /// <param name="test"></param>
  protected virtual void OnAfterCreateHost(ITest test) { }

  /// <summary>
  /// Is the last thing to run as part of <see cref="BeforeTest(ITest)"/>
  /// </summary>
  /// <param name="test"></param>
  protected virtual void OnEndBeforeTest(ITest test) { } 

  /// <summary>
  /// Is the first thing to run as part of <see cref="AfterTest(ITest)"/>
  /// </summary>
  /// <param name="test"></param>
  protected virtual void OnStartAfterTest(ITest test) { } 

  /// <summary>
  /// Is the last thing to run as part of <see cref="AfterTest(ITest)"/>
  /// </summary>
  /// <param name="test"></param>
  protected virtual void OnEndAfterTest(ITest test) { }

  protected TFixture EnsureFixture<TAttribute, TFixture>(ITest test) =>
    (TFixture)EnsureFixture(test, typeof(TAttribute), typeof(TFixture));
  
  protected object EnsureFixture(ITest test, Type attribute, Type fixture)
  {
    if (!test.Fixture!.GetType().IsAssignableTo(fixture))
      throw new Exception(
        $"Test should implement interface '{fixture.FullName}'" +
        $" while also using the attribute '{attribute.FullName}'");

    return test.Fixture;
  }

  public override void BeforeTest(ITest test)
  {
    EnsureFixture(test);
    OnStartBeforeTest(test);
    var builder = CreateHostBuilder();
    
    builder.ConfigureAppConfiguration(DefaultAppConfiguration);
    builder.ConfigureLogging(DefaultLogConfiguration);
    builder.ConfigureServices(DefaultServicesConfiguration);

    if (test.Fixture is IRequireLogValidation)
    {
      builder.ConfigureServices(DefaultLogValidationServices);
      requireLogValidation = true;
    }

    /* now any customizations on the actual fixture */

    if (test.Fixture is IConfigureHostApplication h)
      builder.ConfigureAppConfiguration(h.Apply);

    if (test.Fixture is IConfigureLogging l)
      builder.ConfigureLogging(l.Apply);

    if (test.Fixture is IConfigureServices s)
      builder.ConfigureServices(s.Apply);

    builder.ConfigureServices(
      x => {
        x.AddSingleton(TestContext.CurrentContext);
        x.AddSingleton(test);
      });

    OnBeforeCreateHost(builder, test);

    Fixture.Host = Host = CreateHost(builder);
    configuration = Host.Services.GetRequiredService<IConfiguration>();
    LoggerFactory = Host.Services.GetRequiredService<ILoggerFactory>();

    if (test.Fixture is IRequireLogging)
      ((IRequireLogging)test.Fixture).LoggerFactory = LoggerFactory;
    
    if (test.Fixture is IRequireServices sp)
      sp.Provider = Host.Services;

    OnAfterCreateHost(test);
    
    if (requireLogValidation)
    {
      logContext = Host.Services.GetRequiredService<LogValidationContext>();
      ((IRequireLogValidation)test.Fixture!).LogContext = logContext;
    }

    OnEndBeforeTest(test);
  }

  protected virtual void DefaultAppConfiguration(HostBuilderContext ctx, IConfigurationBuilder config)
  {
    config.AddCommandLine(Environment.GetCommandLineArgs());
    config.AddJsonFile("test.settings.json", optional: true, reloadOnChange: false);
    config.AddJsonFile("test.settings.local.json", optional: true, reloadOnChange: false);
    config.AddEnvironmentVariables("microcelium");
  }

  protected virtual void DefaultLogConfiguration(HostBuilderContext ctx, ILoggingBuilder logging)
  {
    var section = ctx.Configuration.GetSection("Logging");
    if (section != null)
      logging.AddConfiguration(section);

    logging.AddSimpleConsole(
      opt => {
        opt.IncludeScopes = true;
        opt.TimestampFormat = "HH:mm:ss ";
        opt.SingleLine = true;
        opt.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
      });

    logging.AddDebug();

    logging.SetMinimumLevel(LogLevel.Debug);
    logging.AddFilter("Castle", LogLevel.Warning);
    logging.AddFilter("NHibernate", LogLevel.Warning);
    logging.AddFilter("Microsoft", LogLevel.Warning);
    logging.AddFilter("System", LogLevel.Warning);
  }

  protected virtual void DefaultServicesConfiguration(HostBuilderContext ctx, IServiceCollection services) { }
  
  protected virtual void DefaultLogValidationServices(HostBuilderContext ctx, IServiceCollection services)
  {
    services.AddSingleton<ILoggerProvider, LogValidationContextLoggerProvider>();
    services.AddSingleton<LogMessageBuffer>();
    services.AddSingleton<LogValidationContext>();
  }

  public override void AfterTest(ITest test)
  {
    OnStartAfterTest(test);
    OnEndAfterTest(test);
  }

  public override ActionTargets Targets => ActionTargets.Suite;

  private static bool IsSubclassOfGeneric(Type generic, Type? type) =>
    type != null &&
    type != typeof(object) &&
    (generic ==
      (type.IsGenericType
        ? type.GetGenericTypeDefinition()
        : type) ||
      IsSubclassOfGeneric(generic, type.BaseType));
}