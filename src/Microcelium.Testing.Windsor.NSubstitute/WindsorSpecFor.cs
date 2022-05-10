using Microcelium.Testing.Specs;

namespace Microcelium.Testing;

public abstract class
  WindsorSpecFor<TSut, TResult> : AutoMockSpecFor<TSut, TResult, WindsorAutoMockingContainer> 
  where TSut : class { }

public abstract class
  AsyncWindsorSpecFor<TSut, TResult> : AsyncAutoMockSpecFor<TSut, TResult, WindsorAutoMockingContainer>
  where TSut : class { }
