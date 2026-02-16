namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddPick;

internal sealed class AddPickCommandValidator : AbstractValidator<AddPickCommand>
{
  public AddPickCommandValidator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty();
    RuleFor(x => x.Position)
      .NotEmpty();
    RuleFor(x => x.MovieId)
      .NotEmpty();
    RuleFor(x => x.DrafterId)
      .NotEmpty();
  }
}
