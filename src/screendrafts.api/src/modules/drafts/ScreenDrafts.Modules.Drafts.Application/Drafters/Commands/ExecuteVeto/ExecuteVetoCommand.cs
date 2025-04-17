namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVeto;

public sealed record ExecuteVetoCommand(Guid? DrafterTeamId, Guid? DrafterId, Guid PickId, Guid DraftId) : ICommand<Guid>;
