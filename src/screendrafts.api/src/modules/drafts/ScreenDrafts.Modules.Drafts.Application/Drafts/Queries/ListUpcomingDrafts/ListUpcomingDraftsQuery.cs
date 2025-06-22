namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;
public sealed record ListUpcomingDraftsQuery(
  bool IsPatreonOnly,
  Guid UserId,
  bool IsAdmin)
  : IQuery<IReadOnlyList<UpcomingDraftDto>>;

