using FluentAssertions;

using TestResult = NetArchTest.Rules.TestResult;

namespace ScreenDrafts.Common.ArchitectureTests.Abstractions;

public static class TestResultExtensions
{
  public static void ShouldBeSuccessful(this TestResult testResult)
  {
    ArgumentNullException.ThrowIfNull(testResult);

    testResult.FailingTypes?.Should().BeEmpty();
  }
}
