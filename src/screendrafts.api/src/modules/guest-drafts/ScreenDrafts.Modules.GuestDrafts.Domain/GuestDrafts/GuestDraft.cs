namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts;

public sealed class GuestDraft : AggregateRoot<GuestDraftId, Guid>
{
  private readonly List<GuestDraftParticipant> _participants = [];

  private GuestDraft(
    string publicId,
    Guid ownerUserId,
    string title,
    GuestDraftType guestDraftType,
    DateTime createdOnUtc,
    GuestDraftId? id = null
  )
    : base(id ?? GuestDraftId.CreateUnique())
  {
    PublicId = publicId;
    OwnerUserId = ownerUserId;
    Title = title;
    GuestDraftType = guestDraftType;
    GuestDraftStatus = GuestDraftStatus.Created;
    CreatedOnUtc = createdOnUtc;
  }

  private GuestDraft() { }

  public string PublicId { get; private set; } = default!;
  public Guid OwnerUserId { get; private set; }
  public string Title { get; private set; } = default!;
  public GuestDraftType GuestDraftType { get; private set; } = default!;
  public GuestDraftStatus GuestDraftStatus { get; private set; } = default!;
  public string? ShareToken { get; private set; } = default!;
  public DateTime CreatedOnUtc { get; private set; }
  public DateTime? UpdatedOnUtc { get; private set; }
  public IReadOnlyCollection<GuestDraftParticipant> Participants => _participants.AsReadOnly();

  public static Result<GuestDraft> Create(
    string publicId,
    Guid ownerUserId,
    string ownerParticipantPublicId,
    string title,
    GuestDraftType guestDraftType
  )
  {
    if (string.IsNullOrWhiteSpace(title))
    {
      return Result.Failure<GuestDraft>(GuestDraftErrors.TitleIsRequired);
    }

    var guestDraft = new GuestDraft(
      publicId: publicId,
      ownerUserId: ownerUserId,
      title: title,
      guestDraftType: guestDraftType,
      createdOnUtc: DateTime.UtcNow
    );

    guestDraft._participants.Add(
      GuestDraftParticipant.Create(
        publicId: ownerParticipantPublicId,
        guestDraftId: guestDraft.Id,
        userId: ownerUserId,
        isOwner: true
      )
    );

    return Result.Success(guestDraft);
  }

  public Result<GuestDraftParticipant> InviteParticipant(string participantPublicId, Guid userId)
  {
    if (GuestDraftStatus != GuestDraftStatus.Created)
    {
      return Result.Failure<GuestDraftParticipant>(GuestDraftErrors.CannotInviteAfterStart);
    }

    if (_participants.Any(p => p.UserId == userId))
    {
      return Result.Failure<GuestDraftParticipant>(
        GuestDraftErrors.ParticipantAlreadyAdded(userId)
      );
    }

    var participant = GuestDraftParticipant.Create(
      publicId: participantPublicId,
      guestDraftId: Id,
      userId: userId,
      isOwner: false
    );

    _participants.Add(participant);
    UpdatedOnUtc = DateTime.UtcNow;

    return Result.Success(participant);
  }
}
