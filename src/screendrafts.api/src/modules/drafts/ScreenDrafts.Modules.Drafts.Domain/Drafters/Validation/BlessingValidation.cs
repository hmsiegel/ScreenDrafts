namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Validation;

public static class BlessingValidation
{
  public static bool IsValidBlessingRequest(Guid? drafterId, Guid? drafterTeamId) =>
    drafterId.HasValue ^ drafterTeamId.HasValue;
}
