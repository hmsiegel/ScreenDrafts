namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;

public interface IDraftRepository : IRepository<Draft, DraftId>
{
  Task<Draft?> GetByPublicIdWithParticipantsAsync(
    string publicId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Loads the full aggregate graph -- participants, game board + positions, picks
  /// with vetoes/overrides/commissioner overrides. Needed for any gameplay
  /// operation (PlayPick, ApplyVeto, ApplyVetoOverride, ApplyCommissionerOverride,
  /// UndoVeto, UndoPick, RevealPick, Start, Complete, board setup) -- anything
  /// lighter risks a null-reference on a nav path the domain method actually walks.
  /// GetByPublicIdWithParticipantsAsync stays as the cheap path for operations that
  /// only ever touch participants (e.g. InviteParticipant).
  /// </summary>
  Task<Draft?> GetByPublicIdForGameplayAsync(string publicId, CancellationToken cancellationToken);
}
