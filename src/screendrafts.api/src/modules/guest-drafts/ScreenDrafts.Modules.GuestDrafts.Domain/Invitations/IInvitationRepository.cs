namespace ScreenDrafts.Modules.GuestDrafts.Domain.Invitations;

public interface IInvitationRepository : IRepository<Invitation, InvitationId>
{
  Task<Invitation?> GetByDraftAndEmailAsync(
    Guid draftId,
    string normalizedEmail,
    CancellationToken cancellationToken = default
  );

  Task<int> CountForDraftAsync(Guid draftId, CancellationToken cancellationToken = default);

  Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(
    string normalizedEmail,
    CancellationToken cancellationToken = default
  );
}
