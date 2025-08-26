namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.UpdateScreamDrafts;

internal sealed class UpdateScreamDraftsCommandValidator : AbstractValidator<UpdateScreamDraftsCommand>
{
  public UpdateScreamDraftsCommandValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty().WithMessage("DraftId is required.");
    RuleFor(x => x.IsScreamDrafts).NotNull().WithMessage("IsScreamDrafts is required.");
  }
}
