namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetCustomPositions;

internal sealed class Validator : AbstractValidator<SetCustomPositionsCommand>
{
  public Validator()
  {
    RuleFor(x => x.GuestDraftPublicId).NotEmpty();
    RuleFor(x => x.Positions).NotEmpty();
    RuleForEach(x => x.Positions)
      .ChildRules(position =>
      {
        position.RuleFor(p => p.Name).NotEmpty().MaximumLength(GuestDraftPosition.NameMaxLength);
        position.RuleFor(p => p.Picks).NotEmpty();
      });
  }
}
