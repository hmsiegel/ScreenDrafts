namespace ScreenDrafts.Modules.Drafts.Features.People.Edit;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty()
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Person))
      .WithMessage("PublicId is required.");
    RuleFor(x => x.FirstName)
      .NotEmpty().WithMessage("FirstName is required.")
      .MaximumLength(100).WithMessage("FirstName must not exceed 100 characters.");
    RuleFor(x => x.LastName)
      .NotEmpty().WithMessage("LastName is required.")
      .MaximumLength(100).WithMessage("LastName must not exceed 100 characters.");
    RuleFor(x => x.DisplayName)
      .MaximumLength(200).WithMessage("DisplayName must not exceed 200 characters.")
      .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
  }
}
