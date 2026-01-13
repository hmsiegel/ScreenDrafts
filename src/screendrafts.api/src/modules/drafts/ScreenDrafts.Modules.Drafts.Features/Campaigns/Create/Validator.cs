namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed class Validator : AbstractValidator<Request>
{
  public Validator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .MaximumLength(100);
    RuleFor(x => x.Slug)
      .NotEmpty()
      .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
      .WithMessage("Slug must be lowercase alphanumeric characters and hyphens only.")
      .MaximumLength(100);
  }
}


