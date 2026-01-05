namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.PauseDraftPart;

public sealed record PauseDraftPartCommand(Guid DraftPartId) : ICommand;
