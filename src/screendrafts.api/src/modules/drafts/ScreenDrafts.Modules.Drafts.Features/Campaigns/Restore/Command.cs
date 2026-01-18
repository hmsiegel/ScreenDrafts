namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed record Command(string PublicId) : Common.Features.Abstractions.Messaging.ICommand;
