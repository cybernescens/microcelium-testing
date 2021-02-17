using System;
using System.Linq;
using System.Reflection;
using Microcelium.Testing.Selenium.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace Microcelium.Testing.Selenium
{
  /// <summary>
  /// Extensions to facilitate configuration of some tests, particularly tests that implement
  /// a means to configure the <see cref="IServiceCollection"/>
  /// </summary>
  public static class WebSiteExtensions
  {
    private static readonly Type WebSiteType = typeof(IWebSite);
    private static readonly Type WebPageType = typeof(IWebPage);

    /// <summary>
    /// Adds all possible services and the associated implementation for the types provided in <paramref name="types"/>.
    /// If <paramref name="types"/> is null or empty then it searches all loaded assemblies for all types that implement
    /// <see cref="IWebSite"/> and <see cref="IWebPage"/>
    /// </summary>
    /// <param name="services">the <see cref="IServiceCollection"/></param>
    /// <param name="types">a restraining set of types</param>
    /// <returns></returns>
    public static IServiceCollection AddWebComponents(this IServiceCollection services, params Type[] types)
    {
      types = types != null && !types.Any() 
        ? AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).ToArray()
        : types;

      var sites = types
        .Where(x => x.GetInterfaces().Any(y => WebSiteType.IsAssignableFrom(y)) && !x.IsAbstract)
        .Select(x => new {
          Concrete = x, 
          Bases = x.GetInterfaces()
            .Union(x.BaseType != null && x.BaseType.IsAbstract ? new [] {x.BaseType } : Array.Empty<Type>())
            .Union(new [] { x })
        })
        .SelectMany(x => x.Bases, (x, y) => new { Service = y, Implementation = x.Concrete })
        .ToList();

      var pages = types
        .Where(x => x.GetInterfaces().Any(y => WebPageType.IsAssignableFrom(y)) && !x.IsAbstract)
        .Select(x => new {
          Concrete = x,
          Bases = x.GetInterfaces()
            .Union(x.BaseType != null && x.BaseType.IsAbstract ? new[] { x.BaseType } : Array.Empty<Type>())
            .Union(new [] { x })
        })
        .SelectMany(x => x.Bases, (x, y) => new { Service = y, Implementation = x.Concrete })
        .ToList();

      foreach (var site in sites)
        services.AddScoped(site.Service, site.Implementation);

      foreach (var page in pages)
        services.AddScoped(page.Service, page.Implementation);

      return services;
    }
  }
}