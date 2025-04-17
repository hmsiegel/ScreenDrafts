namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.ApplyCommissionerOverride;

public sealed record ApplyCommissionerOverrideCommand(Guid PickId) : ICommand<Guid>;
