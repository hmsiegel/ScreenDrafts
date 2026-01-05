namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateCampaign;

public sealed record CreateCampaignCommand(string Slug, string Name) : ICommand<Guid>;
