namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed record Command(string PersonId) : Common.Features.Abstractions.Messaging.ICommand<string>;
