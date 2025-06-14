namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;

internal sealed class ListDraftsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListDraftsQuery, IReadOnlyCollection<DraftResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyCollection<DraftResponse>>> Handle(
    ListDraftsQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string draftSql =
      $"""
            SELECT
              id AS {nameof(DraftResponse.Id)},
              title AS {nameof(DraftResponse.Title)},
              episode_number AS {nameof(DraftResponse.EpisodeNumber)},
              draft_type AS {nameof(DraftResponse.DraftType)},
              total_picks AS {nameof(DraftResponse.TotalPicks)},
              total_drafters AS {nameof(DraftResponse.TotalDrafters)},
              total_hosts AS {nameof(DraftResponse.TotalHosts)},
              draft_status AS {nameof(DraftResponse.DraftStatus)}
            FROM drafts.drafts
            """;

    const string draftersSql =
      $"""
            SELECT
              dd.draft_id,
              d.id AS {nameof(DrafterResponse.Id)},
              d.name AS {nameof(DrafterResponse.Name)}
            FROM drafts.drafts_drafters dd
            JOIN drafts.drafters d ON dd.drafter_id = d.id
            """;

    const string hostsSql =
      $"""
            SELECT
              dh.hosted_drafts_id,
              h.id AS {nameof(HostResponse.Id)},
              h.host_name AS {nameof(HostResponse.Name)}
            FROM drafts.draft_host dh
            JOIN drafts.hosts h ON dh.hosts_id = h.id
            """;

    const string releaseDatesSql =
      $"""
            SELECT
              rd.draft_id,
              rd.release_date AS {nameof(ReleaseDateResponse.ReleaseDate)}
            FROM drafts.draft_release_date rd
            """;

    var drafts = (await connection.QueryAsync<DraftResponse>(draftSql, request)).ToList();
    var drafters = await connection.QueryAsync<(Guid draft_id, Guid drafter_id, string drafter_name)>(draftersSql);
    var hosts = await connection.QueryAsync<(Guid draft_id, Guid host_id, string host_name)>(hostsSql);
    var releaseDates = await connection.QueryAsync<(Guid draft_id, DateTime release_date)>(releaseDatesSql);

    var draftMap = drafts.ToDictionary(d => d.Id);

    foreach (var (draftId, drafterId, name) in drafters)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddDrafter(new DrafterResponse(drafterId, name));
      }
    }

    foreach(var (draftId, hostId, name) in hosts)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddHost(new HostResponse(hostId, name));
      }
    }

    foreach(var (draftId, date) in releaseDates)
    {
      if (draftMap.TryGetValue(draftId, out var draft))
      {
        draft.AddReleaseDate(new ReleaseDateResponse(DateOnly.FromDateTime(date)));
      }
    }

    return draftMap.Values.ToList();
  }
}
