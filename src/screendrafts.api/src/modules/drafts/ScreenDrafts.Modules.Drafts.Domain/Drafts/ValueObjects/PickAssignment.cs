namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed class PickAssignment
{
  public PickAssignment(
    int position,
    Guid drafterId,
    bool extraVeto = false,
    bool extraVetoOverride = false)
  {
    Position = position;
    DrafterId = drafterId;
    ExtraVeto = extraVeto;
    ExtraVetoOverride = extraVetoOverride;
  }

  private PickAssignment()
  {
  }

  public int Position { get; private set; }

  public Guid DrafterId { get; private set; }

  public Guid? GameBoardId { get; private set; } = default!;

  public bool ExtraVeto { get; private set; }

  public bool ExtraVetoOverride { get; private set; }
}
