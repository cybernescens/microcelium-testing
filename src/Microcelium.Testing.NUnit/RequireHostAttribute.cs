using System;
using Microcelium.Testing.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Microcelium.Testing;

public abstract class RequireHostAttribute : TestActionAttribute 
{
  protected IHost? host;
  protected ILoggerFactory? loggerFactory;
  protected IServiceScope? serviceScope;
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
  /// Runs after <see cref="OnAfterCreateHost"/> and after adding internal context and before <see cref="OnEndBeforeTest(ITest)"/>
  /// </summary>
  protected abstract void ApplyToContext();

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

    builder.ConfigureServices(x => { x.AddSingleton(TestContext.CurrentContext); });

    OnBeforeCreateHost(builder, test);

    Fixture.Host = host = CreateHost(builder);
    configuration = host.Services.GetRequiredService<IConfiguration>();
    loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
    serviceScope = host.Services.CreateScope();

    if (test.Fixture is IRequireLogging logger)
      logger.LoggerFactory = loggerFactory;

    if (test.Fixture is IRequireServices services)
      services.Provider = serviceScope.ServiceProvider;

    OnAfterCreateHost(test);
    
    AddToContext(nameof(IHost), Fixture.Host);
    AddToContext(nameof(IConfiguration), configuration);
    AddToContext(nameof(IServiceProvider), host.Services);
    AddToContext(nameof(ILoggerFactory), loggerFactory);
    AddToContext(nameof(IServiceScope), serviceScope);
    ApplyToContext();

    if (requireLogValidation)
    {
      logContext = serviceScope.ServiceProvider.GetRequiredService<LogValidationContext>();
      ((IRequireLogValidation)test.Fixture!).LogContext = logContext;
      AddToContext(nameof(LogValidationContext), logContext);
    }

    OnEndBeforeTest(test);
  }

  protected static void AddToContext<T>(string key, T value)
  {
    TestExecutionContext.CurrentContext.CurrentTest.Properties.Set($"microcelium_{key}", value!);
  }

  protected virtual void DefaultAppConfiguration(HostBuilderContext ctx, IConfigurationBuilder config)
  {
    config.AddCommandLine(Environment.GetCommandLineArgs());
    config.AddJsonFile("test.settings.json", optional: true, reloadOnChange: false);
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
    SafelyTry.Dispose(() => serviceScope);
    OnEndAfterTest(test);
  }

  public override ActionTargets Targets => ActionTargets.Test;
}