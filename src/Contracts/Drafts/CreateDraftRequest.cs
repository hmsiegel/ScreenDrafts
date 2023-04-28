namespace Contracts.Drafts;
public sealed record CreateDraftRequest(
    string? Name,
    int DraftType,
    int NumberOfDrafters);
