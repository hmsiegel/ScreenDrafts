namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.CreateGameBoard;

internal sealed class CreateGameBoardCommandValidator : AbstractValidator<CreateGameBoardCommand>
{
  public CreateGameBoardCommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty();

    RuleFor(x => x.DraftPartId)
      .NotEmpty();
  }
}
