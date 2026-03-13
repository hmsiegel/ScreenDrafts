namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ClearDraftPositionAssignment;

internal sealed class Validator : AbstractValidator<ClearDraftPositionAssignmentCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Invalid draft part ID format.");

    RuleFor(x => x.PositionPublicId)
      .NotEmpty()
      .WithMessage("Position public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPosition))
      .WithMessage("Invalid position public ID format.");
  }
}
