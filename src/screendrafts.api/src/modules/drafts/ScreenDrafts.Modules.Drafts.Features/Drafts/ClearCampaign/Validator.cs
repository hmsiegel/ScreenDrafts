namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  { 
    RuleFor(x => x.DraftId)
      .NotEmpty()
      .WithMessage("Draft ID must be provided.");
  }
}
