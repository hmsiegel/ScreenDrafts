namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Commands.ExecuteVeto;

public sealed record ExecuteVetoCommand(Guid? DrafterTeamId, Guid? DrafterId, Guid PickId, Guid DraftId) : ICommand<Guid>;
