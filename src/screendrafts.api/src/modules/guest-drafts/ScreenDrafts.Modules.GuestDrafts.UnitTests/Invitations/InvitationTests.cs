using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;
using ScreenDrafts.Modules.GuestDrafts.Domain.Invitations;
using ScreenDrafts.Modules.GuestDrafts.Domain.Invitations.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Invitations;

public class InvitationTests : GuestDraftsBaseTest
{
  private static readonly DateTime Now = new(2026, 10, 11, 12, 0, 0, DateTimeKind.Utc);
  private static readonly TimeSpan Cooldown = TimeSpan.FromHours(24);

  private const string DraftPublicId = "gd_abcdefghijklmno";

  private static Invitation CreateInvitation(
    string email = "bob@example.com",
    string name = "Bob",
    DateTime? utcNow = null,
    Guid? invitedBy = null
  ) =>
    Invitation
      .Create(
        publicId: "gdi_abcdefghijklmno",
        draftId: DraftId.CreateUnique(),
        draftPublicId: DraftPublicId,
        email: email,
        name: name,
        invitedByUserId: invitedBy ?? Guid.NewGuid(),
        utcNow: utcNow ?? Now
      )
      .Value;

  // ── Create ────────────────────────────────────────────────────────────────

  [Fact]
  public void Create_ShouldTrimAndLowercaseEmail_WhenEmailHasPaddingAndMixedCase()
  {
    var invitation = CreateInvitation(email: "  Bob.Smith@Example.COM  ");

    invitation.Email.Should().Be("bob.smith@example.com");
  }

  [Fact]
  public void Create_ShouldTrimName_WhenNameHasPadding()
  {
    var invitation = CreateInvitation(name: "  Bob Smith  ");

    invitation.Name.Should().Be("Bob Smith");
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("not-an-email")]
  [InlineData("bob@")]
  [InlineData("@example.com")]
  [InlineData("Bob <a@b.com>")]
  [InlineData("\"Bob\" <a@b.com>")]
  public void Create_ShouldFailWithInvalidEmail_WhenEmailIsEmptyMalformedOrDisplayNameStyle(
    string email
  )
  {
    var result = Invitation.Create(
      "gdi_abcdefghijklmno",
      DraftId.CreateUnique(),
      DraftPublicId,
      email,
      "Bob",
      Guid.NewGuid(),
      Now
    );

    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle().Which.Should().Be(InvitationErrors.InvalidEmail);
  }

  [Fact]
  public void Create_ShouldFailWithInvalidEmail_WhenEmailExceeds254Characters()
  {
    var email = new string('a', 243) + "@example.com"; // 255 chars

    var result = Invitation.Create(
      "gdi_abcdefghijklmno",
      DraftId.CreateUnique(),
      DraftPublicId,
      email,
      "Bob",
      Guid.NewGuid(),
      Now
    );

    email.Length.Should().Be(255);
    result.Errors.Should().ContainSingle().Which.Should().Be(InvitationErrors.InvalidEmail);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Create_ShouldFailWithInvalidName_WhenNameIsEmptyOrWhitespace(string name)
  {
    var result = Invitation.Create(
      "gdi_abcdefghijklmno",
      DraftId.CreateUnique(),
      DraftPublicId,
      "bob@example.com",
      name,
      Guid.NewGuid(),
      Now
    );

    result.Errors.Should().ContainSingle().Which.Should().Be(InvitationErrors.InvalidName);
  }

  [Fact]
  public void Create_ShouldFailWithInvalidName_WhenNameExceeds100Characters()
  {
    var result = Invitation.Create(
      "gdi_abcdefghijklmno",
      DraftId.CreateUnique(),
      DraftPublicId,
      "bob@example.com",
      new string('n', 101),
      Guid.NewGuid(),
      Now
    );

    result.Errors.Should().ContainSingle().Which.Should().Be(InvitationErrors.InvalidName);
  }

  [Fact]
  public void Create_ShouldSucceed_WhenNameIsExactly100Characters()
  {
    var invitation = CreateInvitation(name: new string('n', 100));

    invitation.Name.Should().HaveLength(100);
  }

  [Fact]
  public void Create_ShouldSucceed_WhenEmailIsExactly254Characters()
  {
    var email = new string('a', 64) + "@" + new string('b', 63) + "." + new string('c', 63) + "." + new string('d', 57) + ".com";
    email.Length.Should().Be(254);

    var result = Invitation.Create(
      "gdi_abcdefghijklmno",
      DraftId.CreateUnique(),
      DraftPublicId,
      email,
      "Bob",
      Guid.NewGuid(),
      Now
    );

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldSetCreatedAndLastSentToTheSameInstantAndBePending()
  {
    var invitation = CreateInvitation();

    invitation.CreatedOnUtc.Should().Be(Now);
    invitation.LastSentOnUtc.Should().Be(invitation.CreatedOnUtc);
    invitation.IsPending.Should().BeTrue();
    invitation.AcceptedOnUtc.Should().BeNull();
    invitation.AcceptedByUserId.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldRaiseExactlyOneInvitationSentEvent_WithTheInvitationDetails()
  {
    var inviter = Guid.NewGuid();

    var invitation = CreateInvitation(
      email: "  Bob@Example.com ",
      name: " Bob ",
      invitedBy: inviter
    );

    var domainEvent = invitation.DomainEvents.Should().ContainSingle().Subject;
    var sent = domainEvent.Should().BeOfType<InvitationSentDomainEvent>().Subject;
    sent.InvitationId.Should().Be(invitation.Id.Value);
    sent.DraftPublicId.Should().Be(DraftPublicId);
    sent.InvitedByUserId.Should().Be(inviter);
    sent.Email.Should().Be("bob@example.com");
    sent.Name.Should().Be("Bob");
  }

  // ── Resend ────────────────────────────────────────────────────────────────

  [Fact]
  public void Resend_ShouldFailWithResendTooSoon_AtOneSecondBeforeTheCooldownEnds()
  {
    var invitation = CreateInvitation();
    invitation.ClearDomainEvents();

    var result = invitation.Resend(DraftPublicId, Now + Cooldown - TimeSpan.FromSeconds(1));

    result.IsFailure.Should().BeTrue();
    result.Error!.Code.Should().Be("Invitations.ResendTooSoon");
    invitation.LastSentOnUtc.Should().Be(Now);
    invitation.DomainEvents.Should().BeEmpty();
  }

  [Fact]
  public void Resend_ShouldSucceed_AtExactly24Hours()
  {
    var invitation = CreateInvitation();

    var result = invitation.Resend(DraftPublicId, Now + Cooldown);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void Resend_ShouldUpdateLastSentAndRaiseTheEvent_WhenAllowed()
  {
    var invitation = CreateInvitation(email: "bob@example.com", name: "Bob");
    invitation.ClearDomainEvents();
    var resentAt = Now + Cooldown + TimeSpan.FromMinutes(5);

    invitation.Resend(DraftPublicId, resentAt);

    invitation.LastSentOnUtc.Should().Be(resentAt);
    invitation.CreatedOnUtc.Should().Be(Now);
    var sent = invitation
      .DomainEvents.Should()
      .ContainSingle()
      .Subject.Should()
      .BeOfType<InvitationSentDomainEvent>()
      .Subject;
    sent.InvitationId.Should().Be(invitation.Id.Value);
    sent.DraftPublicId.Should().Be(DraftPublicId);
    sent.Email.Should().Be("bob@example.com");
    sent.Name.Should().Be("Bob");
    sent.InvitedByUserId.Should().Be(invitation.InvitedByUserId);
  }

  [Fact]
  public void Resend_ShouldFailWithAlreadyAccepted_WhenInvitationWasAccepted()
  {
    var invitation = CreateInvitation();
    invitation.Accept(Guid.NewGuid(), Now.AddHours(1));
    invitation.ClearDomainEvents();

    var result = invitation.Resend(DraftPublicId, Now + Cooldown + TimeSpan.FromDays(1));

    result.Error.Should().Be(InvitationErrors.AlreadyAccepted);
    invitation.DomainEvents.Should().BeEmpty();
  }

  [Fact]
  public void Resend_ShouldStartANewCooldown_FromTheResendInstant()
  {
    var invitation = CreateInvitation();
    var resentAt = Now + Cooldown;
    invitation.Resend(DraftPublicId, resentAt);

    var second = invitation.Resend(DraftPublicId, resentAt + Cooldown - TimeSpan.FromSeconds(1));

    second.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void ResendAvailableOnUtc_ShouldBeLastSentPlus24Hours()
  {
    var invitation = CreateInvitation();

    invitation.ResendAvailableOnUtc.Should().Be(Now + Cooldown);

    var resentAt = Now + Cooldown;
    invitation.Resend(DraftPublicId, resentAt);

    invitation.ResendAvailableOnUtc.Should().Be(resentAt + Cooldown);
  }

  // ── Accept ────────────────────────────────────────────────────────────────

  [Fact]
  public void Accept_ShouldRecordWhoAndWhenAndStopBeingPending()
  {
    var invitation = CreateInvitation();
    var userId = Guid.NewGuid();
    var acceptedAt = Now.AddHours(2);

    var result = invitation.Accept(userId, acceptedAt);

    result.IsSuccess.Should().BeTrue();
    invitation.AcceptedOnUtc.Should().Be(acceptedAt);
    invitation.AcceptedByUserId.Should().Be(userId);
    invitation.IsPending.Should().BeFalse();
  }

  [Fact]
  public void Accept_ShouldFailWithAlreadyAcceptedAndLeaveFieldsUnchanged_WhenCalledTwice()
  {
    var invitation = CreateInvitation();
    var firstUser = Guid.NewGuid();
    var firstAt = Now.AddHours(2);
    invitation.Accept(firstUser, firstAt);

    var result = invitation.Accept(Guid.NewGuid(), firstAt.AddDays(3));

    result.Error.Should().Be(InvitationErrors.AlreadyAccepted);
    invitation.AcceptedByUserId.Should().Be(firstUser);
    invitation.AcceptedOnUtc.Should().Be(firstAt);
  }

  // ── NormalizeEmail ────────────────────────────────────────────────────────

  [Fact]
  public void NormalizeEmail_ShouldReturnEmpty_WhenNull() =>
    Invitation.NormalizeEmail(null!).Should().BeEmpty();

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("\t \n")]
  public void NormalizeEmail_ShouldReturnEmpty_WhenWhitespace(string email) =>
    Invitation.NormalizeEmail(email).Should().BeEmpty();

  [Theory]
  [InlineData("Bob@Example.COM", "bob@example.com")]
  [InlineData("  bOb@example.com\t", "bob@example.com")]
  [InlineData("ALREADY@lower.com", "already@lower.com")]
  public void NormalizeEmail_ShouldTrimAndLowercase(string input, string expected) =>
    Invitation.NormalizeEmail(input).Should().Be(expected);
}
