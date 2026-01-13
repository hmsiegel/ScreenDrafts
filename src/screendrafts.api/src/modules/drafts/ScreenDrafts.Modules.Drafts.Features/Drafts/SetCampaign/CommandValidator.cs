namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed class CommandValidator : AbstractValidator<Command>
{
  public CommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty().WithMessage("Draft ID must be provided.");
    RuleFor(x => x.CampaignId)
      .NotEmpty().WithMessage("Campaign ID must be provided.");
  }
}
