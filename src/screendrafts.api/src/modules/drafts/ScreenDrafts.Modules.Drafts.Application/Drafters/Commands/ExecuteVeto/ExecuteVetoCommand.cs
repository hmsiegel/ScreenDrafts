namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVeto;

public sealed record ExecuteVetoCommand(Guid DrafterId, Guid PickId) : ICommand;
