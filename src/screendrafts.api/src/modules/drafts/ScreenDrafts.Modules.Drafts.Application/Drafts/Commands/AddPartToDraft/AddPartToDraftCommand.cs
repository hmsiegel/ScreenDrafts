namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPartToDraft;

public sealed record AddPartToDraftCommand(Guid DraftId, int PartIndex) : ICommand;
