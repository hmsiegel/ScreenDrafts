namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;

/// <summary>
/// A stable, module-local identity for anyone who's been registered as a Guest
/// Drafter -- mirrors canonical Drafter, minus the Person indirection (not needed
/// here; GuestDrafter wraps UserId directly). This is deliberately its own
/// aggregate, not a shared/extracted Person entity.
/// Created by consuming UserRegisteredIntegrationEvent (mirrors how Drafts'
/// Person is created via the same event), kept fresh via
/// UserNameUpdatedIntegrationEvent -- both live in the Features layer, not here.
/// </summary>
public sealed class Drafter : AggregateRoot<DrafterId, Guid>
{
  private Drafter(
    string publicId,
    Guid userId,
    string firstName,
    string lastName,
    DrafterId? id = null
  )
    : base(id ?? DrafterId.CreateUnique())
  {
    PublicId = publicId;
    UserId = userId;
    FirstName = firstName;
    LastName = lastName;
  }

  private Drafter() { }

  public string PublicId { get; private set; } = default!;
  public Guid UserId { get; private set; }
  public string FirstName { get; private set; } = default!;
  public string LastName { get; private set; } = default!;

  public string DisplayName => $"{FirstName} {LastName}".Trim();

  public static Result<Drafter> Create(
    string publicId,
    Guid userId,
    string firstName,
    string lastName,
    DrafterId? id = null
  )
  {
    if (string.IsNullOrWhiteSpace(firstName))
    {
      return Result.Failure<Drafter>(DrafterErrors.InvalidFirstName);
    }

    if (string.IsNullOrWhiteSpace(lastName))
    {
      return Result.Failure<Drafter>(DrafterErrors.InvalidLastName);
    }

    return new Drafter(publicId, userId, firstName, lastName, id);
  }

  public Result UpdateName(string firstName, string lastName)
  {
    if (string.IsNullOrWhiteSpace(firstName))
    {
      return Result.Failure(DrafterErrors.InvalidFirstName);
    }

    if (string.IsNullOrWhiteSpace(lastName))
    {
      return Result.Failure(DrafterErrors.InvalidLastName);
    }

    FirstName = firstName;
    LastName = lastName;

    return Result.Success();
  }
}
