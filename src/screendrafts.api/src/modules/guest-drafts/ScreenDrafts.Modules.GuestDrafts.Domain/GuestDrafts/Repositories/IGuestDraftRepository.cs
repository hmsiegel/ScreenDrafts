namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Repositories;

public interface IGuestDraftRepository : IRepository<GuestDraft, GuestDraftId>
{
  Task<GuestDraft?> GetByPublicIdWithParticipantsAsync(
    string publicId,
    CancellationToken cancellationToken = default
  );
}
