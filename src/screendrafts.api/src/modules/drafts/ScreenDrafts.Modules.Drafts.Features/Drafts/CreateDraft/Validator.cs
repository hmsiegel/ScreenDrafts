namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.DraftType).IsInEnum();
    RuleFor(x => x.SeriesId).NotEqual(Guid.Empty);
    RuleFor(x => x.TotalPicks).GreaterThanOrEqualTo(5);
    RuleFor(x => x)
      .Must(x => x.TotalDrafters + x.TotalDrafterTeams >= 2)
      .WithMessage("The sum of TotalDrafters and TotalDrafterTeams must be at least 2.");
  }
}
