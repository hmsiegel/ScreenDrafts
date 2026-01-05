namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ContinueDraftPart;

public sealed record ContinueDraftPartCommand(Guid DraftPartId) : ICommand;
