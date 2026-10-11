using ValidationResult = FluentValidation.Results.ValidationResult;

using InviteValidator = ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Invite.Validator;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations.Invite;

public sealed class InviteGuestDrafterValidatorTests
{
  private const string ValidDraftId = "gd_abcdefghijklmno";

  private static ValidationResult Validate(
    string draftId = ValidDraftId,
    string email = "bob@example.com",
    string name = "Bob"
  ) =>
    new InviteValidator().Validate(
      new InviteGuestDrafterCommand
      {
        GuestDraftPublicId = draftId,
        CallerUserPublicId = "u_abcdefghijklmno",
        Email = email,
        Name = name,
      }
    );

  [Fact]
  public void Validate_ShouldPass_WhenAllFieldsAreValid() =>
    Validate().IsValid.Should().BeTrue();

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("gdi_abcdefghijklmno")]
  [InlineData("gd_short")]
  [InlineData("abcdefghijklmno")]
  public void Validate_ShouldFail_WhenDraftPublicIdIsEmptyOrMalformed(string draftId)
  {
    var result = Validate(draftId: draftId);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == nameof(InviteGuestDrafterCommand.GuestDraftPublicId));
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("not-an-email")]
  [InlineData("bob@")]
  [InlineData("@example.com")]
  public void Validate_ShouldFail_WhenEmailIsEmptyOrInvalid(string email)
  {
    var result = Validate(email: email);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == nameof(InviteGuestDrafterCommand.Email));
  }

  [Fact]
  public void Validate_ShouldFail_WhenEmailExceeds254Characters()
  {
    var result = Validate(email: new string('a', 243) + "@example.com");

    result.Errors.Should().Contain(e => e.PropertyName == nameof(InviteGuestDrafterCommand.Email));
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Validate_ShouldFail_WhenNameIsEmpty(string name)
  {
    var result = Validate(name: name);

    result.Errors.Should().Contain(e => e.PropertyName == nameof(InviteGuestDrafterCommand.Name));
  }

  [Fact]
  public void Validate_ShouldFail_WhenNameExceeds100Characters()
  {
    var result = Validate(name: new string('n', 101));

    result.Errors.Should().Contain(e => e.PropertyName == nameof(InviteGuestDrafterCommand.Name));
  }

  [Fact]
  public void Validate_ShouldPass_WhenNameIsExactly100Characters() =>
    Validate(name: new string('n', 100)).IsValid.Should().BeTrue();
}
