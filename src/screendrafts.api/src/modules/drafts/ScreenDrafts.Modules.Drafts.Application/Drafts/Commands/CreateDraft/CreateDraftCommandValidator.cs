namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

internal sealed class CreateDraftCommandValidator : AbstractValidator<CreateDraftCommand>
{
  public CreateDraftCommandValidator()
  {
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.DraftType).NotNull();
    RuleFor(x => x.TotalPicks).GreaterThanOrEqualTo(5);
    RuleFor(x => x)
      .Must(x => x.TotalDrafters + x.TotalDrafterTeams >= 2)
      .WithMessage("The sum of TotalDrafters and TotalDrafterTeams must be at least 2.");
  }
}
