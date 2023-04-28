namespace ScreenDrafts.Application.Drafts.Commands.CreateDraft;
internal sealed class CreateDraftCommandValidator : AbstractValidator<CreateDraftCommand>
{
    public CreateDraftCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.DraftType)
            .NotEmpty();
        RuleFor(x => x.NumberOfDrafters)
            .NotEmpty()
            .GreaterThan(0);
    }
}
