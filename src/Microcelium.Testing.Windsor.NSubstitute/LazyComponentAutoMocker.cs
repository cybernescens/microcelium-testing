using System;
using System.Collections;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using NSubstitute;

namespace Microcelium.Testing;

public class LazyComponentAutoMocker : ILazyComponentLoader
{
  public IRegistration Load(string name, Type service, Arguments arguments) =>
    Component.For(service).Instance(Substitute.For(new[] { service }, Array.Empty<object>()));

  public IRegistration Load(string key, Type service, IDictionary arguments) =>
    Component.For(service).Instance(Substitute.For(new[] { service }, Array.Empty<object>()));
}