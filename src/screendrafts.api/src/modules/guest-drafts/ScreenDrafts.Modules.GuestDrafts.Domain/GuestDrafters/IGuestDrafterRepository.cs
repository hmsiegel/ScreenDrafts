namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafters;

public interface IGuestDrafterRepository : IRepository<GuestDrafter, GuestDrafterId>
{
  Task<GuestDrafter?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

  /// <summary>
  /// Live text search by display name -- backs the search-for-guests endpoint.
  /// GuestDrafter's DisplayName is kept fresh via UserRegisteredIntegrationEvent/
  /// UserNameUpdatedIntegrationEvent, so this can be a genuine local query rather
  /// than proxying IUsersApi at read time.
  /// </summary>
  Task<IReadOnlyList<GuestDrafter>> SearchAsync(
    string? search,
    CancellationToken cancellationToken
  );
}
