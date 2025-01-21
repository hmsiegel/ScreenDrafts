namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public static class DraftErrors
{
  public static Error NotFound(Ulid draftId) =>
    new($"Draft with id {draftId} was not found.");

  public static Error TooManyDrafters =>
    new("Cannot add more drafters than the total allowed.");

  public static Error DrafterAlreadyAddes(Ulid drafterId) =>
    new($"Drafter with id {drafterId} is already added to the draft.");

  public static Error PickPositionAlreadyTaken(int position) =>
    new($"Pick position {position} is already taken.");

  public static Error HostAlreadyAdded(Ulid hostId) =>
    new($"Host with id {hostId} is already added to the draft.");

  public static Error DraftNotStarted =>
    new("Cannot add picks unless the draft has started.");

  public static Error TooManyHosts =>
    new("Cannot add more hosts than the total allowed.");

  public static Error CannotVetoUnlessTheDraftIsStarted =>
    new("Cannot veto a pick unless the draft has started.");

  public static Error CannotVetoAPickThatDoesNotExist =>
    new("Cannot veto a pick that does not exist.");

  public static Error CannotVetoAPickThatIsNotYours =>
    new("Cannot veto a pick that is not yours.");

  public static Error CannotVetoAPickThatIsAlreadyVetoed =>
    new("Cannot veto a pick that is already vetoed.");

  public static Error OnlyDraftersInTheDraftCanUseAVeto =>
    new("Only drafters in the draft can use a veto.");

  public static Error DraftCanOnlyBeStartedIfItIsCreated =>
    new("Draft can only be started if it is created.");

  public static Error CannotStartDraftWithoutAllDrafters =>
    new("Cannot start the draft without all drafters.");

  public static Error CannotStartDraftWithoutAllHosts =>
    new("Cannot start the draft without all hosts.");

  public static Error CannotCompleteDraftIfItIsNotInProgress =>
    new("Cannot complete the draft if it is not in progress.");

  public static Error CannotCompleteDraftWithoutAllPicks =>
    new("Cannot complete the draft without all picks.");

  public static Error CannotVetoOverrideAVetoThatDoesNotExist =>
    new("Cannot veto override a veto that does not exist.");

  public static Error OnlyDraftersInTheDraftCanUseAVetoOverride =>
    new("Only drafters in the draft can use a veto override.");
}
