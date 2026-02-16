namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.ApplyCommissionerOverride;

public sealed record ApplyCommissionerOverrideCommand(Guid PickId) : ICommand<Guid>;
