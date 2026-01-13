using ICommand = ScreenDrafts.Common.Features.Abstractions.Messaging.ICommand;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed record Command : ICommand
{
  public string DraftId { get; init; } = default!;
}
