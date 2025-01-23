namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

internal sealed class CreateDraftCommandValidator : AbstractValidator<CreateDraftCommand>
{
  public CreateDraftCommandValidator()
  {
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.DraftType).NotNull();
    RuleFor(x => x.TotalPicks).GreaterThan(3);
    RuleFor(x => x.TotalDrafters).GreaterThan(1);
    RuleFor(x => x.TotalHosts).GreaterThan(0);
  }
}
