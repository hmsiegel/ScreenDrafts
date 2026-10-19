using ContactValidator = ScreenDrafts.Modules.Communications.Features.ContactMessages.SubmitContactMessage.Validator;

namespace ScreenDrafts.Modules.Communications.IntegrationTests.ContactMessages;

public sealed class SubmitContactMessageValidatorTests
{
  private readonly ContactValidator _validator = new();

  [Fact]
  public void Validate_ShouldPass_ForAValidCommand()
  {
    _validator.Validate(BuildCommand()).IsValid.Should().BeTrue();
  }

  [Theory]
  [InlineData("Bug")]
  [InlineData("Question")]
  [InlineData("Feedback")]
  [InlineData("AccountHelp")]
  [InlineData("bug")]
  [InlineData("ACCOUNTHELP")]
  public void Validate_ShouldPass_ForEveryKnownCategory(string category)
  {
    _validator.Validate(BuildCommand(category: category)).IsValid.Should().BeTrue();
  }

  [Theory]
  [InlineData("")]
  [InlineData("Complaint")]
  [InlineData("99")]
  public void Validate_ShouldFail_ForAnUnknownCategory(string category)
  {
    _validator.Validate(BuildCommand(category: category)).IsValid.Should().BeFalse();
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Validate_ShouldFail_WhenNameIsBlank(string name)
  {
    _validator.Validate(BuildCommand(name: name)).IsValid.Should().BeFalse();
  }

  [Fact]
  public void Validate_ShouldFail_WhenNameIsTooLong()
  {
    var name = new string('a', ContactMessage.MaxNameLength + 1);

    _validator.Validate(BuildCommand(name: name)).IsValid.Should().BeFalse();
  }

  [Theory]
  [InlineData("")]
  [InlineData("not-an-email")]
  [InlineData("missing-domain@")]
  public void Validate_ShouldFail_WhenEmailIsInvalid(string email)
  {
    _validator.Validate(BuildCommand(email: email)).IsValid.Should().BeFalse();
  }

  [Fact]
  public void Validate_ShouldFail_WhenMessageIsTooShort()
  {
    _validator.Validate(BuildCommand(message: "short")).IsValid.Should().BeFalse();
  }

  [Fact]
  public void Validate_ShouldFail_WhenMessageIsTooLong()
  {
    var message = new string('a', ContactMessage.MaxMessageLength + 1);

    _validator.Validate(BuildCommand(message: message)).IsValid.Should().BeFalse();
  }

  [Fact]
  public void Validate_ShouldPass_WhenMessageIsExactlyAtTheLimit()
  {
    var message = new string('a', ContactMessage.MaxMessageLength);

    _validator.Validate(BuildCommand(message: message)).IsValid.Should().BeTrue();
  }

  [Fact]
  public void Validate_ShouldFail_WhenTurnstileTokenIsMissing()
  {
    _validator.Validate(BuildCommand(turnstileToken: "")).IsValid.Should().BeFalse();
  }

  private static SubmitContactMessageCommand BuildCommand(
    string name = "Jane Doe",
    string email = "jane@example.com",
    string category = "Question",
    string message = "How do I change my email address?",
    string turnstileToken = "valid-token"
  )
  {
    return new SubmitContactMessageCommand
    {
      Name = name,
      Email = email,
      Category = category,
      Message = message,
      TurnstileToken = turnstileToken,
    };
  }
}
