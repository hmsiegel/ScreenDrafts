namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Search;

// ── ASSUMPTION FLAG ──────────────────────────────────────────────────────────
internal sealed class SearchDraftsQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IUsersApi usersApi,
  IDrafterRepository drafterRepository
) : IQueryHandler<SearchDraftsQuery, PagedResult<GuestDraftSummaryResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IDrafterRepository _drafterRepository = drafterRepository;

  public async Task<Result<PagedResult<GuestDraftSummaryResponse>>> Handle(
    SearchDraftsQuery request,
    CancellationToken cancellationToken
  )
  {
    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure<PagedResult<GuestDraftSummaryResponse>>(
        UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId)
      );
    }

    // Nullable by design — a caller who's never registered as a Drafter
    // can still own drafts (ownership is UserId-based, independent of the
    // participant system). When this is null, the participant half of the
    // WHERE clause below just never matches anything, which is correct.
    var callerDrafter = await _drafterRepository.GetByUserIdAsync(caller.UserId, cancellationToken);

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string baseSql = $"""
      SELECT
        gd.public_id  AS {nameof(DraftRow.PublicId)},
        gd.title      AS {nameof(DraftRow.Title)},
        gd.guest_draft_type       AS {nameof(DraftRow.Type)},
        gd.guest_draft_status     AS {nameof(DraftRow.Status)},
        gd.draft_date AS {nameof(DraftRow.DraftDate)},
        (gd.owner_user_id = @CallerUserId) AS {nameof(DraftRow.IsOwner)}
      FROM guest_drafts.drafts gd
      WHERE (
        gd.owner_user_id = @CallerUserId
        OR (
          @CallerDrafterId IS NOT NULL
          AND EXISTS (
            SELECT 1
            FROM guest_drafts.draft_participants p
            WHERE p.draft_id = gd.id
              AND p.participant_id_value = @CallerDrafterId
              AND p.participant_kind_value = 0
          )
        )
      )

      """;

    var sqlBuilder = new StringBuilder(baseSql);
    var parameters = new DynamicParameters();
    parameters.Add("CallerUserId", caller.UserId, DbType.Guid);
    parameters.Add("CallerDrafterId", callerDrafter?.Id.Value, DbType.Guid);

    if (!string.IsNullOrWhiteSpace(request.Status))
    {
      // gd.guest_draft_status is an integer column (the SmartEnum's Value) --
      // the request carries the enum's Name, so it must be resolved before
      // binding, not passed through as text (Postgres has no integer = text
      // operator and would throw at query time).
      if (!DraftStatus.TryFromName(request.Status, ignoreCase: true, out var status))
      {
        return Result.Failure<PagedResult<GuestDraftSummaryResponse>>(
          DraftErrors.InvalidStatus(request.Status)
        );
      }

      sqlBuilder.Append(" AND gd.guest_draft_status = @Status");
      parameters.Add("Status", status.Value, DbType.Int32);
    }

    sqlBuilder.Append(" ORDER BY gd.draft_date DESC NULLS LAST, gd.title ASC");

    // S2077: sqlBuilder is our own app-built query text (fixed literal clauses only); all values are bound via Dapper parameters above.
#pragma warning disable S2077
    var totalCount = await connection.ExecuteScalarAsync<int>(
      new CommandDefinition(
        $"SELECT COUNT(*) FROM ({sqlBuilder}) sub",
        parameters,
        cancellationToken: cancellationToken
      )
    );
#pragma warning restore S2077

    var pageSize = Math.Min(request.PageSize, 100);
    var skip = (Math.Max(request.Page, 1) - 1) * pageSize;

    parameters.Add("PageSize", pageSize);
    parameters.Add("Skip", skip);
    sqlBuilder.Append(" LIMIT @PageSize OFFSET @Skip");

    var rows = (
      await connection.QueryAsync<DraftRow>(
        new CommandDefinition(
          sqlBuilder.ToString(),
          parameters,
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var items = rows.Select(r => new GuestDraftSummaryResponse
      {
        PublicId = r.PublicId,
        Title = r.Title,
        Type = DraftType.FromValue(r.Type).Name,
        Status = DraftStatus.FromValue(r.Status).Name,
        DraftDate = r.DraftDate,
        IsOwner = r.IsOwner,
      })
      .ToList();

    return Result.Success(
      new PagedResult<GuestDraftSummaryResponse>
      {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = pageSize,
      }
    );
  }

  private sealed record DraftRow
  {
    public string PublicId { get; init; } = default!;
    public string Title { get; init; } = default!;
    public int Type { get; init; } = default!;
    public int Status { get; init; } = default!;
    public DateOnly? DraftDate { get; init; } = default!;
    public bool IsOwner { get; init; } = default!;
  }
}
