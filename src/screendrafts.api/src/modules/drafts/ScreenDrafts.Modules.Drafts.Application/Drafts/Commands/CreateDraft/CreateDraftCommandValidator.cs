namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

internal sealed class CreateDraftCommandValidator : AbstractValidator<CreateDraftCommand>
{
  public CreateDraftCommandValidator()
  {
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.DraftType).NotNull();
    RuleFor(x => x.NumberOfDrafters).GreaterThan(0);
    RuleFor(x => x.NumberOfCommissioners).GreaterThan(0);
    RuleFor(x => x.NumberOfMovies).GreaterThan(0);
  }
}
