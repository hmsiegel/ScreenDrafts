namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateGameBoard;

internal sealed class CreateGameBoardCommandValidator : AbstractValidator<CreateGameBoardCommand>
{
  public CreateGameBoardCommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty();
  }
}
