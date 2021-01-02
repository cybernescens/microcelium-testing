# Writing Test

## Terms

### SUT (Subject Under Test)

Tests should always focus on a single Subject, or SUT. The SUT is defined by the context of the test, therefore the SUT could be a single component or a whole service.

### Sociable or Solitary

* Sociable - Tests that use real dependency implementations
* Solitary - Dependencies are substituted for fake ones that can have preconfigured responses.

Neither of these are bad and can even be used in combination. Use of substitutes comes with its own price of complexity. I would prefer to us a real component until either the complexity of configuring the component and asserting the outcome becomes more complicated than using a substitution. Or the component introduces side effects that cause additonal complexity in the test.

### Substitutions (Test Doubles)

We use the word Mocks and Stubs interchangable but they do mean different things. Martin Fowler has a good artical on the different types ([Test Doubles](https://martinfowler.com/bliki/TestDouble.html))

In simple terms though a stub is canned responses and a mock is for validating that certain methods where called. In otherwords a stub provides responses which are used by the SUT where as a mock validates that the SUT did what it was sopposed to. 

A good rule to follow is to only use mocks to assert expectations as a last resort, when there is no other way of validating that something has happened. The reason for this is that the test should not dictate *how* the code does what it is supposed to do, but *that* it meets the functional requirements.

### Assertions

This is how we afferm that the test has passed. Each test should test one thing and one functional item only. This helps identify the individual facets of what we are testing and helps identify the failure point.

```c#
[Test]
public void TestEverything() \\This is bad
{
  var sut = new Sut();
  var result = sut.DoYourThing(x);
  Assert.That(result, Is...);

  result = sut.DoYourThing(y);
  Assert.That(result, Is...);

  result = sut.DoYourThing(z);
  Assert.That(result, Is...);
}

[Test]
public void TestMethodCall() \\Avoid this (more on this later)
{
  var sut = new Sut();
  var result = sut.DoYourThing(x);

  Assert.That(result.ValueA, Is...);
  Assert.That(result.ValueB, Is...);
  Assert.That(result.ValueC, Is...);
}

[Test]
public void GetCollectionOfData() \\This is ok
{
  var sut = new Sut();
  var results = sut.GetItems();

  Assert.That(result.Count(), Has...);
  Assert.That(result, Contains(...));
}
```

## Types of tests

### Unit Test

Whilst this seems like a simple topic and perhaps superflus it is necessery to outline what a unit test is to give context to the next sections.

A unit test encapsulates the functionality of the SUT using either the real or subsituted dependencies and then the results are validted to ensure that we have the correct outcome.

All types of testing follow the same pattern; Arrange, Act and Assert.

* Arrange - The arrangement is where we set up the scenario.
* Act - Invoke the thing we are testing on the SUT.
* Assert - Validate that our expections are met.

Unit tests are useful to write around existing functunality that is not already under test prior to making changes or testing newly added functionality. If you write a unit test it does not mean that you are doint TDD, see next section.

### TDD (Test Driven Development)

TDD is a way of writting unit tests. In TDD we write the test prior to the implementation. In the case of writing a test for a new component or methid we use the test to drive out what the names of the new items are but the implementations do nothing or throw `NotImplementedException`s. If the components and methods already exist we write out the test that defines the expected result before we do the implementation.

The tests should be run to confirm that they fail prior to implementation, this is a process called [Red Green Refactor](https://www.codecademy.com/articles/tdd-red-green-refactor).

TDD is a great process that helps flesh out components and methods, ensuring the minimal amount of code is written to ensure that the coponent does what it is supposed to do and there by ensuring the [YAGNI](https://en.wikipedia.org/wiki/You_aren%27t_gonna_need_it) principle is adhered to.

ReSharper is a great TDD tool, it allows you to write the test first and generate the code from the test and move it into the right location with out having to do all the usual cruft.

### BDD (Behavior Driven Testing)

BDD is a subset of TDD but BDD is more than just testing. It tries to capture the business intent behind the software. Basic unit tests tend to be more technical in nature, they define the outcome but don't explain why the outcome is needed or of business value.

BDD facilitates conversations and understanding.

## Examples