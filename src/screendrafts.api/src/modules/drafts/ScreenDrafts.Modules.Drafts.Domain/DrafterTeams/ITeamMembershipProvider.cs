namespace ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

/// <summary>
/// Infrastructure service for reading a DrafterTeam's CURRENT membership without loading
/// the full DrafterTeam aggregate. Same reasoning as IDraftPolicyProvider: a command
/// handler operating on a different aggregate (here, DraftPart/Pick) needs one small fact
/// from DrafterTeam, not the whole thing.
///
/// "Current" is the operative word — this is deliberately a live read, used only at the
/// instant a team pick is created to snapshot membership onto TeamPickCredit. Nothing
/// should call this later and expect it to reflect who was on the team in the past;
/// membership drifts (drafters graduate into Legends teams over time), which is exactly
/// why the snapshot exists instead of a live join at read time.
/// </summary>
public interface ITeamMembershipProvider
{
  Task<IReadOnlyList<Guid>> GetCurrentMemberDrafterIdsAsync(
    Guid drafterTeamId,
    CancellationToken cancellationToken
  );
}
