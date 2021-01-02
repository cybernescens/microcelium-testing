using System;

namespace Microcelium.Testing.Specs
{
  /// <summary>
  /// Basic Flow for performing Arrange - Act - Assert testing
  /// </summary>
  /// <typeparam name="TSut">the type of System Under Test</typeparam>
  /// <typeparam name="TResult">the type of result Result</typeparam>
  public abstract class SpecsFor<TSut, TResult>
  {
    /// <summary>
    /// The Subject that is is under test, created by <see cref="CreateSubject"/>
    /// </summary>
    protected TSut Subject { get; private set; }

    /// <summary>
    /// The object obtained executing <see cref="Act"/>
    /// </summary>
    protected TResult Result { get; private set; }

    /// <summary>
    /// Instantiates our System Under Test
    /// </summary>
    /// <returns>an instantiated object representing our System Under Test</returns>
    protected abstract TSut CreateSubject();

    /// <summary>
    /// After <see cref="CreateSubject"/>, we can perform any custom configuration of our <see cref="Subject"/> (System Under Test)
    /// </summary>
    /// <param name="createSubject"></param>
    /// <returns></returns>
    protected virtual TSut Arrange(Func<TSut> createSubject) => createSubject();

    /// <summary>
    /// After <see cref="Arrange"/>, this is where we perform the action we are testing against our <see cref="Subject"/> (System Under Test)
    /// </summary>
    /// <param name="subject">the instantiated and arranged System Under Test</param>
    /// <returns>the result obtained from invoking our action to test against the <paramref name="subject"/> (System Under Test)</returns>
    protected abstract TResult Act(TSut subject);

    /* member is invoked from Microcelium.Testing.NUnit.Specs.SpecAttribute for example */
    // ReSharper disable once UnusedMember.Global
    internal void Run()
    {
      Subject = Arrange(CreateSubject);
      Result = Act(Subject);
    }

    /// <summary>
    /// Allows any customized cleanup
    /// </summary>
    protected virtual void TearDown() { }

    /// <summary>
    /// Deconstructs and disposes of this object
    /// </summary>
    ~SpecsFor() => TearDown();
  }
}