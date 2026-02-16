namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddDraftPositionsToGameBoard;

internal sealed class AddDraftPositionsToGameBoardCommandValidator : AbstractValidator<AddDraftPositionsToGameBoardCommand>
{
  public AddDraftPositionsToGameBoardCommandValidator()
  {
    RuleFor(x => x.GameBoardId)
      .NotEmpty()
      .WithMessage("Game board ID cannot be empty.");
    RuleFor(x => x.DraftPositionRequests)
      .NotEmpty()
      .WithMessage("Draft position IDs cannot be empty.");
  }
}
