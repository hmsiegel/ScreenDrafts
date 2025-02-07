using FluentAssertions;

using NetArchTest.Rules;

namespace ScreenDrafts.Common.ArchitectureTests.Abstractions;

public static class TestResultExtensions
{
  public static void ShouldBeSuccessful(this TestResult testResult)
  {
    ArgumentNullException.ThrowIfNull(testResult);

    testResult.FailingTypes?.Should().BeEmpty();
  }
}
