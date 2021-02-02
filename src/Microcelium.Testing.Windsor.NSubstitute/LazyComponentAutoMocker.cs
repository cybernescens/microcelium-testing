using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using NSubstitute;

namespace Microcelium.Testing
{
  internal class LazyComponentAutoMocker : ILazyComponentLoader
  {
    public IRegistration Load(string name, Type service, Arguments arguments) =>
      Component.For(service).Instance(Substitute.For(new[] {service}, null));
  }
}