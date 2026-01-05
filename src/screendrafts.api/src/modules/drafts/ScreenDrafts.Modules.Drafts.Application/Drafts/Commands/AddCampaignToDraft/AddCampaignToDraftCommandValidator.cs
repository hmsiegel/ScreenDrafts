namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCampaignToDraft;

internal sealed class AddCampaignToDraftCommandValidator : AbstractValidator<AddCampaignToDraftCommand>
{
  public AddCampaignToDraftCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
    RuleFor(x => x.CampaignId).NotEmpty();
  }
}

