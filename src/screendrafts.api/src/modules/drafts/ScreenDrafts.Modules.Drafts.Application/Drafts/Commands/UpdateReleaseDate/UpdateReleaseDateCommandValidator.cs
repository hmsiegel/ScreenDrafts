namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.UpdateReleaseDate;

internal sealed class UpdateReleaseDateCommandValidator : AbstractValidator<UpdateReleaseDateCommand>
{
  public UpdateReleaseDateCommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty();
    RuleFor(x => x.ReleaseDate)
      .NotEmpty();
  }
}
