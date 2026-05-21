namespace ScreenDrafts.Modules.Drafts.PublicApi;

public interface IDraftsApi
{
  Task<IReadOnlyList<MediaAppearanceRecord>> GetMediaAppearancesAsync(
    string mediaPublicId,
    bool includePatreon,
    CancellationToken ct = default
  );
}
