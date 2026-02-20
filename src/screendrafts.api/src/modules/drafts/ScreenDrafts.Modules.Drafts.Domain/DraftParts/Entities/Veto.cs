using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class Veto : Entity<VetoId>
{
  private Veto(
    Pick pick,
    DraftPartParticipant issuedByParticipant,
    DateTime? occurredOn = null,
    string? note = null,
    VetoId? id = null)
    : base(id ?? VetoId.CreateUnique())
  {
    TargetPick = pick;
    TargetPickId = pick.Id;
    IssuedByParticipant = issuedByParticipant;
    IssuedByParticipantId = issuedByParticipant.Id;
    OccurredOn = occurredOn;
    Note = note;
  }

  private Veto()
  {
  }

  public Pick TargetPick { get; private set; } = default!;
  public PickId TargetPickId { get; private set; } = default!;

  public DraftPart DraftPart => TargetPick.DraftPart;
  public DraftPartId DraftPartId => TargetPick.DraftPartId;

  public DraftPartParticipant IssuedByParticipant { get; private set; } = default!;
  public DraftPartParticipantId IssuedByParticipantId { get; private set; } = default!;

  public bool IsOverridden { get; private set; }

  public VetoOverride? VetoOverride { get; private set; } = default!;

  public DateTime? OccurredOn { get; private set; }
  public string? Note { get; private set; } = default!;


  public static Result<Veto> Create(
    Pick pick,
    DraftPartParticipant issuedByParticipant,
    VetoId? id = null,
    string? note = null)
  {
    if (pick is null)
    {
      return Result.Failure<Veto>(VetoErrors.PickMustBeProvided);
    }

    ArgumentNullException.ThrowIfNull(issuedByParticipant);
    ArgumentNullException.ThrowIfNull(pick);

    var veto = new Veto(
      pick: pick,
      issuedByParticipant: issuedByParticipant,
      occurredOn: DateTime.UtcNow,
      note: note,
      id: id ?? VetoId.CreateUnique());

    veto.Raise(new VetoCreatedDomainEvent(
      veto.Id.Value,
      pick.Id.Value,
      issuedByParticipant.ParticipantId));

    return veto;
  }

  internal static Result<Veto> SeedCreate(
    Pick pick,
    DraftPartParticipant issuedByParticipant,
    DateTime occurredOn,
    VetoId? id = null,
    string? note = null)
  {
    var veto = new Veto(
      id: id,
      pick: pick,
      occurredOn: occurredOn,
      issuedByParticipant: issuedByParticipant,
      note: note);
    return veto;
  }

  public Result Override(Participant by)
  {
    if (IsOverridden)
    {
      return Result.Failure(VetoErrors.VetoOverrideAlreadyUsed);
    }

    var participant = DraftPartParticipant.Create(DraftPart, by);

    IsOverridden = true;
    VetoOverride = VetoOverride.Create(
      veto: this,
      issuedByParticipant: participant).Value;

    return Result.Success();
  }
}
