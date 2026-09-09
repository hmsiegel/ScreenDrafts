using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Create;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Create;

internal sealed class Validator : AbstractValidator<CreateDraftCommand>
{
  public Validator()
  {
    RuleFor(x => x.OwnerUserPublicId)
      .NotEmpty()
      .WithMessage("OwnerUserPublicId is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.User))
      .WithMessage("OwnerUserPublicId must be a valid public ID with the correct prefix.");

    RuleFor(x => x.Title).NotEmpty().MaximumLength(Draft.TitleMaxLength);

    RuleFor(x => x.Type).NotEmpty();
  }
}
