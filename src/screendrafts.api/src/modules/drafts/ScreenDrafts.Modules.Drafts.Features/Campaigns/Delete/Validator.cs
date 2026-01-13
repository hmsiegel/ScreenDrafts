namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed class Validator : AbstractValidator<Request>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty().WithMessage("PublicId is required.");
  }
}
