namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

internal sealed class SetReleaseDateValidator : AbstractValidator<SetReleaseDateCommand>
{
  public SetReleaseDateValidator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty()
      .Must(id => PublicIdGuards.IsValidWithPrefix(id,PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part ID is required.");

    RuleFor(x => x.ReleaseChannel)
      .Must(x => ReleaseChannel.List.Any(rc => rc.Value == x.Value))
      .WithMessage("Invalid release channel.");
  }
}
