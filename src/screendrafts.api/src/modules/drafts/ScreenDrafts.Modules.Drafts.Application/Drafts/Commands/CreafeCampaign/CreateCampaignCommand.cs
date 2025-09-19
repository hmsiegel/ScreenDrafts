namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreafeCampaign;

public sealed record CreateCampaignCommand(string Slug, string Name) : ICommand<Guid>;
