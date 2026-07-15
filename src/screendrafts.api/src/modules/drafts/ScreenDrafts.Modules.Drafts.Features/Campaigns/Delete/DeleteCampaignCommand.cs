namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed record DeleteCampaignCommand : ICommand
{
  public required string PublicId { get; init; }
}
