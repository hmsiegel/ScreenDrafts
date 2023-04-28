namespace ScreenDrafts.Application.Drafts.Commands.CreateDraft;
public sealed record CreateDraftCommand(
    string? Name,
    int DraftType,
    int NumberOfDrafters) : IRequest<DefaultIdType>;
