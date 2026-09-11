using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Positions.AssignParticipantToPosition;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Positions.AssignParticipantToPosition;

internal sealed class Validator : AbstractValidator<AssignParticipantToPositionCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId)
      .NotEmpty()
      .WithMessage("GuestDraftPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraft))
      .WithMessage("GuestDraftPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.PositionPublicId)
      .NotEmpty()
      .WithMessage("PositionPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDraftPosition))
      .WithMessage("PositionPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.GuestDrafterPublicId)
      .NotEmpty()
      .WithMessage("GuestDrafterPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.GuestDrafter))
      .WithMessage("GuestDrafterPublicId must be a valid public ID with the correct prefix.");
  }
}
