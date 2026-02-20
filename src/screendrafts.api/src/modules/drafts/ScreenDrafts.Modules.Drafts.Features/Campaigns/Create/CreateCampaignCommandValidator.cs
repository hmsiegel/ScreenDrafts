namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
  public CreateCampaignCommandValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Campaign name is required.")
      .MaximumLength(100)
      .WithMessage("Campaign name must not exceed 100 characters.");

    RuleFor(x => x.Slug)
      .NotEmpty()
      .WithMessage("Campaign slug is required.")
      .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
      .WithMessage("Slug must be lowercase alphanumeric characters and hyphens only.")
      .MaximumLength(100)
      .WithMessage("Campaign slug must not exceed 100 characters.");
  }
}



