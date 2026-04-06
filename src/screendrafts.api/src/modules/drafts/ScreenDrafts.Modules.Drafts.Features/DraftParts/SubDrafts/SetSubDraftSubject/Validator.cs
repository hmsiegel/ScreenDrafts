namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSubDraftSubject;

internal sealed class Validator : AbstractValidator<SetSubDraftSubjectCommand>
{
  public Validator()
  {
    RuleFor(x => x.DraftPartPublicId)
      .NotEmpty()
      .WithMessage("Draft part public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.DraftPart))
      .WithMessage("Draft part public ID must start with the correct prefix.");

    RuleFor(x => x.SubDraftPublicId)
      .NotEmpty()
      .WithMessage("Sub-draft public ID is required.")
      .Must(id => PublicIdGuards.IsValidWithPrefix(id, PublicIdPrefixes.SubDraft))
      .WithMessage("Sub-draft public ID must start with the correct prefix.");

    RuleFor(x => x.SubjectKind)
      .MustBeSmartEnumValue<SetSubDraftSubjectCommand, SubjectKind>()
      .WithMessage("Invalid subject kind.");

    RuleFor(x => x.SubjectName)
      .NotEmpty()
      .WithMessage("Subject name is required.")
      .MaximumLength(100)
      .WithMessage("Subject name must not exceed 100 characters.");
  }
}
