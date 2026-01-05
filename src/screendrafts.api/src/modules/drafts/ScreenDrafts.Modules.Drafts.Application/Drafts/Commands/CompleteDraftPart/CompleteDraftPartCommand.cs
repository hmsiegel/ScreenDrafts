namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CompleteDraftPart;

public sealed record CompleteDraftPartCommand(Guid DraftPartId) : ICommand;
