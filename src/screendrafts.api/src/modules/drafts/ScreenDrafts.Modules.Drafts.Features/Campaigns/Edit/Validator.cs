namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty().WithMessage("Campaign ID must be provided.");

    RuleFor(x => x.Name)
      .MaximumLength(100).WithMessage("Campaign name must not exceed 100 characters.")
      .When(x => x.Name is not null);

    RuleFor(x => x.Slug)
      .Matches("^[a-z0-9-]+$").WithMessage("Campaign slug can only contain lowercase letters, numbers, and hyphens.")
      .MaximumLength(50).WithMessage("Campaign slug must not exceed 50 characters.")
      .When(x => x.Slug is not null);

    RuleFor(x => x)
      .Must(x => x.Name is not null || x.Slug is not null)
      .WithMessage("At least one field (Name or Slug) must be provided for update.");
  }
}
