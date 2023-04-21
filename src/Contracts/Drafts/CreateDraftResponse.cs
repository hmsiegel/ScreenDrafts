namespace Contracts.Drafts;
public sealed record CreateDraftResponse(
    string Id,
    string Name,
    DraftType DraftType,
    int EpisodeNumber);
