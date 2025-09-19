namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPartToDraft;

internal sealed class AddPartToDraftCommandValidator : AbstractValidator<AddPartToDraftCommand>
{
  public AddPartToDraftCommandValidator()
  {
    RuleFor(x => x.DraftId)
      .NotEmpty().WithMessage("DraftId is required.")
      .Must(id => Guid.TryParse(id.ToString(), out _))
      .WithMessage("DraftId must be a valid GUID.");

    RuleFor(x => x.PartIndex)
      .GreaterThan(0).WithMessage("PartIndex must be greater than zero.");
  }
}
