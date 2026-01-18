namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed record Command(string PublicId) : Common.Features.Abstractions.Messaging.ICommand;
