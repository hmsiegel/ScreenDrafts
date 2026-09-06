namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafters;

/// <summary>
/// A stable, module-local identity for anyone who's been registered as a Guest
/// Drafter -- mirrors canonical Drafter, minus the Person indirection (not needed
/// here; GuestDrafter wraps UserId directly). This is deliberately its own
/// aggregate, not a shared/extracted Person entity.
/// Created by consuming UserRegisteredIntegrationEvent (mirrors how Drafts'
/// Person is created via the same event), kept fresh via
/// UserNameUpdatedIntegrationEvent -- both live in the Features layer, not here.
/// </summary>
public sealed class GuestDrafter : AggregateRoot<GuestDrafterId, Guid>
{
  private GuestDrafter(
    string publicId,
    Guid userId,
    string firstName,
    string lastName,
    GuestDrafterId? id = null
  )
    : base(id ?? GuestDrafterId.CreateUnique())
  {
    PublicId = publicId;
    UserId = userId;
    FirstName = firstName;
    LastName = lastName;
  }

  private GuestDrafter() { }

  public string PublicId { get; private set; } = default!;
  public Guid UserId { get; private set; }
  public string FirstName { get; private set; } = default!;
  public string LastName { get; private set; } = default!;

  public string DisplayName => $"{FirstName} {LastName}".Trim();

  public static Result<GuestDrafter> Create(
    string publicId,
    Guid userId,
    string firstName,
    string lastName,
    GuestDrafterId? id = null
  )
  {
    if (string.IsNullOrWhiteSpace(firstName))
    {
      return Result.Failure<GuestDrafter>(GuestDrafterErrors.InvalidFirstName);
    }

    if (string.IsNullOrWhiteSpace(lastName))
    {
      return Result.Failure<GuestDrafter>(GuestDrafterErrors.InvalidLastName);
    }

    return new GuestDrafter(publicId, userId, firstName, lastName, id);
  }

  public Result UpdateName(string firstName, string lastName)
  {
    if (string.IsNullOrWhiteSpace(firstName))
    {
      return Result.Failure(GuestDrafterErrors.InvalidFirstName);
    }

    if (string.IsNullOrWhiteSpace(lastName))
    {
      return Result.Failure(GuestDrafterErrors.InvalidLastName);
    }

    FirstName = firstName;
    LastName = lastName;

    return Result.Success();
  }
}
