namespace Contracts.Drafts;
public sealed record AddDrafterRequest(
    DefaultIdType DraftId,
    DefaultIdType DrafterId);
