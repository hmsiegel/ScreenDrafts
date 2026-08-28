namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedRevealPick;

internal sealed class Validator : AbstractValidator<SeedRevealPickCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .WithMessage("Draft part ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID must be a valid public ID with the correct prefix.");

    RuleFor(x => x.PlayOrder).GreaterThan(0).WithMessage("Play order must be greater than 0.");

    // Who reveals a pick differs by draft shape: a Host's public ID on a hosted part, a
    // Drafter's on a hostless one (see Pick.RevealAuthorizedParticipant's remarks and
    // RevealPickCommandHandler, which resolves the same duality on the live path). This
    // seeding endpoint has no way to know in advance which kind the caller will send, so
    // it accepts either prefix rather than assuming Host.
    RuleFor(x => x.ActedByPublicId)
      .NotEmpty()
      .WithMessage("A host or drafter public ID is required.")
      .Must(id =>
        PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Host)
        || PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.Drafter)
      )
      .WithMessage(
        "The acting public ID must be a valid host or drafter public ID with the correct prefix."
      );
  }
}
