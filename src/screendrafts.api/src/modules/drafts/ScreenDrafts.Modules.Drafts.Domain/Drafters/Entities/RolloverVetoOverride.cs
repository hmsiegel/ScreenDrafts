namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class RolloverVetoOverride : Entity<RolloverVetoOverrideId>
{
  private RolloverVetoOverride(
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    Guid fromDraftId,
    RolloverVetoOverrideId? id = null)
  {
    Id = id ?? RolloverVetoOverrideId.CreateUnique();

    Drafter = drafter;
    DrafterId = drafter?.Id;

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam?.Id;

    FromDraftId = fromDraftId;
    IsUsed = false;
  }

  private RolloverVetoOverride()
  {
  }

  public Drafter? Drafter { get; private set; } = default!;
  public DrafterId? DrafterId { get; private set; } = default!;

  public DrafterTeam? DrafterTeam { get; private set; } = default!;
  public DrafterTeamId? DrafterTeamId { get; private set; } = default!;


  public Guid FromDraftId { get; private set; }

  public Guid? ToDraftId { get; private set; }

  public bool IsUsed { get; private set; }

  public static Result<RolloverVetoOverride> Create(
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    Guid fromDraftId)
  {
    if (drafter is not null && drafterTeam is not null)
    {
      return Result.Failure<RolloverVetoOverride>(
        RolloverVetoOverrideErrors.DrafterAndDrafterTeamCannotBeBothSet);
    }

    if (drafter is null && drafterTeam is null)
    {
      return Result.Failure<RolloverVetoOverride>(
        RolloverVetoOverrideErrors.DrafterAndDrafterTeamCannotBeNull);
    }

    var rolloverVeto = new RolloverVetoOverride(drafter, drafterTeam, fromDraftId);

    rolloverVeto.Raise(new RolloverVetoOverrideCreatedDomainEvent(
      rolloverVeto.Id.Value,
      drafter?.Id.Value,
      drafterTeam?.Id.Value,
      fromDraftId));

    return rolloverVeto;
  }

  public Result Use(Guid toDraftId)
  {
    if (IsUsed)
    {
      return Result.Failure(RolloverVetoOverrideErrors.RolloverVetoOverrideAlreadyUsed);
    }

    ToDraftId = toDraftId;
    IsUsed = true;
    Raise(new RolloverVetoOverrideUsedDomainEvent(Id.Value, toDraftId));

    return Result.Success();
  }
}
