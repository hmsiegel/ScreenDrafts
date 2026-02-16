namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Commands.ExecuteVetoOverride;
public sealed record ExecuteVetoOverrideCommand(Guid? DrafterId, Guid? DrafterTeamId, Guid VetoId) : ICommand<Guid>;
