namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddReleaseToDraftPart;

internal sealed class AddReleaseToDraftPartCommandValidator : AbstractValidator<AddReleaseToDraftPartCommand>
{
  public AddReleaseToDraftPartCommandValidator()
  {
    RuleFor(x => x.DraftPartId)
      .NotEmpty().WithMessage("Draft part ID is required.");
    RuleFor(x => x.ReleaseChannel)
      .IsInEnum().WithMessage("Invalid release channel.");
    RuleFor(x => x.ReleaseDate)
      .NotEmpty().WithMessage("Release date is required.");
  }
}
