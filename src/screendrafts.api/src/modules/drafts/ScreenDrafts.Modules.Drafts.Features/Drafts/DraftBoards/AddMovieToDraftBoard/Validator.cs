namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.AddMovieToDraftBoard;

internal sealed class Validator : AbstractValidator<AddMovieToDraftBoardCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty()
      .WithMessage("Draft ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Draft))
      .WithMessage("Invalid Draft ID format.");

    RuleFor(x => x.UserPublicId)
      .NotEmpty()
      .WithMessage("User Public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.User))
      .WithMessage("Invalid User Public ID format.");

    RuleFor(x => x.TmdbId)
     .GreaterThan(0)
     .WithMessage("TMDB ID must be a positive integer.");

    RuleFor(x => x.Notes)
      .MaximumLength(1000)
      .WithMessage("Notes cannot exceed 1000 characters.");

    RuleFor(x => x.Priority)
      .GreaterThan(0)
      .When(x => x.Priority.HasValue)
      .WithMessage("Priority must be a positive integer.");
  }
}
