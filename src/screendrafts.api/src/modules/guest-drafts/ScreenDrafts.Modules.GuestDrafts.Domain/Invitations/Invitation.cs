using System.Net.Mail;

using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;
using ScreenDrafts.Modules.GuestDrafts.Domain.Invitations.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.Invitations;

/// <summary>
/// A pending invite for someone without an account. When a user registers with the
/// invited email, the Accept flow adds them to the draft as a participant.
/// </summary>
public sealed class Invitation : AggregateRoot<InvitationId, Guid>
{
  public const int EmailMaxLength = 254;
  public const int NameMaxLength = 100;
  public const int MaxPerDraft = 10;
  public static readonly TimeSpan ResendCooldown = TimeSpan.FromHours(24);

  private Invitation(
    string publicId,
    DraftId draftId,
    string email,
    string name,
    Guid invitedByUserId,
    DateTime utcNow,
    InvitationId? id = null
  )
    : base(id ?? InvitationId.CreateUnique())
  {
    PublicId = publicId;
    DraftId = draftId;
    Email = email;
    Name = name;
    InvitedByUserId = invitedByUserId;
    CreatedOnUtc = utcNow;
    LastSentOnUtc = utcNow;
  }

  private Invitation() { }

  public string PublicId { get; private set; } = default!;
  public DraftId DraftId { get; private set; } = default!;
  public string Email { get; private set; } = default!;
  public string Name { get; private set; } = default!;
  public Guid InvitedByUserId { get; private set; }
  public DateTime CreatedOnUtc { get; private set; }
  public DateTime LastSentOnUtc { get; private set; }
  public DateTime? AcceptedOnUtc { get; private set; }
  public Guid? AcceptedByUserId { get; private set; }

  public bool IsPending => AcceptedOnUtc is null;

  public DateTime ResendAvailableOnUtc => LastSentOnUtc + ResendCooldown;

  public static string NormalizeEmail(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();

  public static Result<Invitation> Create(
    string publicId,
    DraftId draftId,
    string draftPublicId,
    string email,
    string name,
    Guid invitedByUserId,
    DateTime utcNow,
    InvitationId? id = null
  )
  {
    var normalizedEmail = NormalizeEmail(email);

    if (
      normalizedEmail.Length is 0 or > EmailMaxLength
      || !MailAddress.TryCreate(normalizedEmail, out var parsed)
      || !string.Equals(parsed.Address, normalizedEmail, StringComparison.Ordinal)
    )
    {
      return Result.Failure<Invitation>(InvitationErrors.InvalidEmail);
    }

    var trimmedName = (name ?? string.Empty).Trim();

    if (trimmedName.Length is 0 or > NameMaxLength)
    {
      return Result.Failure<Invitation>(InvitationErrors.InvalidName);
    }

    var invitation = new Invitation(
      publicId,
      draftId,
      normalizedEmail,
      trimmedName,
      invitedByUserId,
      utcNow,
      id
    );

    invitation.Raise(
      new InvitationSentDomainEvent(
        invitationId: invitation.Id.Value,
        draftPublicId: draftPublicId,
        invitedByUserId: invitedByUserId,
        email: invitation.Email,
        name: invitation.Name
      )
    );

    return invitation;
  }

  public Result Resend(string draftPublicId, DateTime utcNow)
  {
    if (!IsPending)
    {
      return Result.Failure(InvitationErrors.AlreadyAccepted);
    }

    if (utcNow < ResendAvailableOnUtc)
    {
      return Result.Failure(InvitationErrors.ResendTooSoon(ResendAvailableOnUtc));
    }

    LastSentOnUtc = utcNow;

    Raise(
      new InvitationSentDomainEvent(
        invitationId: Id.Value,
        draftPublicId: draftPublicId,
        invitedByUserId: InvitedByUserId,
        email: Email,
        name: Name
      )
    );

    return Result.Success();
  }

  public Result Accept(Guid userId, DateTime utcNow)
  {
    if (!IsPending)
    {
      return Result.Failure(InvitationErrors.AlreadyAccepted);
    }

    AcceptedOnUtc = utcNow;
    AcceptedByUserId = userId;

    return Result.Success();
  }
}
