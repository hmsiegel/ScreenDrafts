namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed record CreateDrafterCommand(string PersonId) : ICommand<string>;

