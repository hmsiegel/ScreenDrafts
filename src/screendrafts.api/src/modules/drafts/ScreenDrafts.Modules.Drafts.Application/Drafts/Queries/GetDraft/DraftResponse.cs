namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

public sealed record DraftResponse(
    Ulid Id,
    string Title,
    string DraftType,
    int TotalPicks,
    int TotalDrafters,
    int TotalHosts,
    string DraftStatus);
