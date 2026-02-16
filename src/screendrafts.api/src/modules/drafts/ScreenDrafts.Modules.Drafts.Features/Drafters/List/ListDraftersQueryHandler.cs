namespace ScreenDrafts.Modules.Drafts.Features.Drafters.List;

internal sealed class ListDraftersQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<ListDraftersQuery, DrafterCollectionResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<DrafterCollectionResponse>> Handle(ListDraftersQuery ListDraftersRequest, CancellationToken cancellationToken)
  {
    await using var conn = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var r = ListDraftersRequest.ListDraftersRequest;

    var page = r.Page ?? 1;
    var pageSize = r.PageSize ?? 10;
    var offset = (page - 1) * pageSize;

    var orderBy = (r.Sort, r.Direction) switch
    {
      ("name", "desc") => "p.display_name desc",
      _ => "p.display_name asc"
    };

    var args = new
    {
      r.Q,
      r.Retired,
      r.PageSize,
      Offset = offset,
    };

    const string baseWhere =
      $"""
      where
        (
          @Retired = 'all'
          or (@Retired = 'active' and d.is_retired = false)
          or (@Retired = 'retired' and d.is_retired = true)
        )
        and (
          @Q is null
          or p.display_name ILIKE ('%' || @Q || '%')
        )
      """;

    const string countSql =
      $"""
      select count(*)::bigint
      from drafts.drafters d
      join drafts.people p on p.id = d.person_id
      {baseWhere}
      """;

    var itemsSql =
      $"""
        select
          d.public_id as {nameof(DrafterListItem.DrafterId)},
          p.public_id as {nameof(DrafterListItem.PersonId)},
          p.display_name as {nameof(DrafterListItem.DisplayName)},
          d.is_retired as {nameof(DrafterListItem.IsRetired)}
        from drafts.drafters d
        join drafts.people p on p.id = d.person_id
        {baseWhere}
        order by {orderBy}
        limit @PageSize offset @Offset;
      """;

    var total = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
      countSql,
      args,
      cancellationToken: cancellationToken));

    var items = (await conn.QueryAsync<DrafterListItem>(new CommandDefinition(
      itemsSql,
      args,
      cancellationToken: cancellationToken))).AsList();

    return Result.Success(new DrafterCollectionResponse(
      new PagedResult<DrafterListItem>
      {
        Items = new Collection<DrafterListItem>(items),
        TotalCount = total,
        Page = page,
        PageSize = pageSize
      }));

  }
}



