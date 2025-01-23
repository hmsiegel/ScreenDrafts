namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraft;

public sealed record DraftResponse(
    Guid Id,
    string Title,
    string DraftType,
    int TotalPicks,
    int TotalDrafters,
    int TotalHosts,
    string DraftStatus);
