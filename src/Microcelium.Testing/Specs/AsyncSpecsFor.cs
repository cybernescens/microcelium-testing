using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microcelium.Testing.Specs;

/// <summary>
///   Basic Flow for performing Arrange - Act - Assert testing, where Act is performed Asynchronously
/// </summary>
/// <typeparam name="TSut">the type of System Under Test</typeparam>
/// <typeparam name="TResult">the type of result Result</typeparam>
public abstract class AsyncSpecsFor<TSut, TResult> : IRequireHost
{
  /// <summary>
  ///   The Subject that is is under test, created by <see cref="CreateSubject" />
  /// </summary>
  protected TSut? Subject { get; private set; }

  /// <summary>
  ///   The object obtained executing <see cref="Act" />
  /// </summary>
  protected TResult? Result { get; private set; }

  /// <summary>
  ///   Instantiates our System Under Test
  /// </summary>
  /// <returns>an instantiated object representing our System Under Test</returns>
  protected abstract Task<TSut> CreateSubject();

  /// <summary>
  ///   After <see cref="CreateSubject" />, we can perform any custom configuration
  ///   of our <see cref="Subject" /> (System Under Test)
  /// </summary>
  /// <param name="createSubject"></param>
  /// <returns></returns>
  protected virtual async Task<TSut> Arrange(/*Func<Task<TSut>> createSubject*/)
  {
    await ArrangeBeforeCreate();
    var subject = await CreateSubject();
    await ArrangeAfterCreate(subject);
    return subject;
  }

  /// <summary>
  /// Arrange called prior to <see cref="CreateSubject"/>
  /// </summary>
  protected virtual Task ArrangeBeforeCreate() => Task.CompletedTask;

  /// <summary>
  /// Arrange called after <see cref="CreateSubject"/>
  /// </summary>
  /// <param name="subject">the subject or System Under Test</param>
  protected virtual Task ArrangeAfterCreate(TSut subject) => Task.CompletedTask;

  /// <summary>
  ///   After <see cref="SpecsFor{TSut,TResult}.Arrange" />, this is where
  ///   we perform the action we are testing against our
  ///   <see cref="SpecsFor{TSut,TResult}.Subject" /> (System Under Test)
  /// </summary>
  /// <param name="subject">the instantiated and arranged System Under Test</param>
  /// <returns>
  ///   the result obtained from invoking our action to test against the
  ///   <paramref name="subject" /> (System Under Test)
  /// </returns>
  protected abstract Task<TResult> Act(TSut subject);

  /* member is invoked from Microcelium.Testing.NUnit.Specs.SpecAttribute for example */
  // ReSharper disable once UnusedMember.Global
  internal async Task Run()
  {
    Subject = await Arrange();
    Result = await Act(Subject);
  }

  /// <summary>
  ///   Allows any customized cleanup
  /// </summary>
  protected virtual Task TearDown() => Task.CompletedTask;

  /// <summary>
  ///   Deconstructs and disposes of this object
  /// </summary>
  ~AsyncSpecsFor() { TearDown(); }

  public IHost Host { get; set; }
}