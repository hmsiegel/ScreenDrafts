namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Get;

internal sealed class Validator : AbstractValidator<Query>
{
  public Validator()
  {
    RuleFor(x => x.DrafterId)
      .NotEmpty()
      .Must(drafterId => PublicIdGuards.IsValidWithPrefix(drafterId, PublicIdPrefixes.Drafter))
      .WithMessage($"DrafterId must start with the '{PublicIdPrefixes.Drafter}' prefix.");
  }
}
