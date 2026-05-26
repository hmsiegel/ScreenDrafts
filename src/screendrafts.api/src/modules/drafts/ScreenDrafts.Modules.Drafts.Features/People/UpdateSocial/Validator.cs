namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateSocial;

internal sealed class Validator : AbstractValidator<UpdateSocialCommand>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .WithMessage("PublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Person))
      .WithMessage("PublicId is not valid.");
    RuleFor(x => x.TwitterHandle).MaximumLength(15).WithMessage("Twitter handle is too long.");
    RuleFor(x => x.InstagramHandle).MaximumLength(30).WithMessage("Instagram handle is too long.");
    RuleFor(x => x.LetterboxdHandle)
      .MaximumLength(30)
      .WithMessage("Letterboxd handle is too long.");
    RuleFor(x => x.BlueskyHandle).MaximumLength(30).WithMessage("Bluesky handle is too long.");
  }
}
