namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreafeCampaign;

internal sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
  public CreateCampaignCommandValidator()
  {
    RuleFor(x => x.Slug)
      .NotEmpty().WithMessage("Slug should not be empty.");
    RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Name should not be empty.");
  }
}
