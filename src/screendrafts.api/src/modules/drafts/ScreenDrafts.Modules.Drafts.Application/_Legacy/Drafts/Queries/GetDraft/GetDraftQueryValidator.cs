namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraft;

internal sealed class GetDraftQueryValidator : AbstractValidator<GetDraftQuery>
{
  public GetDraftQueryValidator()
  {
    RuleFor(x => x.DraftId).NotEmpty();
  }
}
