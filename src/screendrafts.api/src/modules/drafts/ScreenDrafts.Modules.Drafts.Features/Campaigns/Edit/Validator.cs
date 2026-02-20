namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;

internal sealed class Validator : AbstractValidator<EditCampaignCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Campaign))
      .WithMessage("Campaign ID must be provided.");

    RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Campaign name cannot be empty.")
      .MaximumLength(100).WithMessage("Campaign name must not exceed 100 characters.")
      .When(x => x.Name is not null);

    RuleFor(x => x.Slug)
      .NotEmpty().WithMessage("Campaign slug cannot be empty.")
      .Matches("^[a-z0-9-]+$").WithMessage("Campaign slug can only contain lowercase letters, numbers, and hyphens.")
      .MaximumLength(50).WithMessage("Campaign slug must not exceed 50 characters.")
      .When(x => x.Slug is not null);
  }
}

