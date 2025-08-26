namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.UpdateScreamDrafts;

public sealed record UpdateScreamDraftsCommand(
    Guid DraftId,
    bool IsScreamDrafts) : ICommand;
