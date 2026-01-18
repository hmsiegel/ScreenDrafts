namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed record Command(string PublicId) : Common.Features.Abstractions.Messaging.ICommand;
