namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class RolloverVeto : Entity<RolloverVetoId>
{
  private RolloverVeto(
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    Guid fromDraftId,
    RolloverVetoId? id = null)
  {
    Id = id ?? RolloverVetoId.CreateUnique();

    Drafter = drafter;
    DrafterId = drafter?.Id;

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam?.Id;

    FromDraftId = fromDraftId;
    IsUsed = false;
  }

  private RolloverVeto()
  {
  }

  public DrafterId? DrafterId { get; private set; } = default!;
  public Drafter? Drafter { get; private set; } = default!;

  public DrafterTeam? DrafterTeam { get; private set; } = default!;
  public DrafterTeamId? DrafterTeamId { get; private set; } = default!;

  public Guid FromDraftId { get; private set; }
  public Guid? ToDraftId { get; private set; }

  public bool IsUsed { get; private set; }

  public static Result<RolloverVeto> Create(
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    Guid fromDraftId)
  {
    if (fromDraftId == Guid.Empty)
    {
      return Result.Failure<RolloverVeto>(RolloverVetoErrors.InvalidDraftId);
    }

    if (drafter is null && drafterTeam is null)
    {
      return Result.Failure<RolloverVeto>(RolloverVetoErrors.InvalidDrafterOrTeam);
    }

    if (drafter is not null && drafterTeam is not null)
    {
      return Result.Failure<RolloverVeto>(RolloverVetoErrors.InvalidDrafterAndTeam);
    }

    var rolloverVeto = new RolloverVeto(
      drafter,
      drafterTeam,
      fromDraftId);

    rolloverVeto.Raise(new RolloverVetoCreatedDomainEvent(
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
      return Result.Failure(RolloverVetoErrors.RolloverVetoAlreadyUsed);
    }

    ToDraftId = toDraftId;
    IsUsed = true;
    Raise(new RolloverVetoUsedDomainEvent(Id.Value, toDraftId));

    return Result.Success();
  }
}
