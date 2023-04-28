namespace ScreenDrafts.Application.Drafts.Commands.AddDraftersToDraft;
public sealed record AddDrafterToDraftCommand(
    DefaultIdType DrafterId,
    DefaultIdType DraftId) : IRequest<ErrorOr<AddDrafterReponse>>;
