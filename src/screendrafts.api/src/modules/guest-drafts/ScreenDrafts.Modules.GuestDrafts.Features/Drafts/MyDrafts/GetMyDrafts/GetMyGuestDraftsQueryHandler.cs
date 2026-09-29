namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.MyDrafts.GetMyDrafts;

// ── Handler ───────────────────────────────────────────────────────────────────
internal sealed class GetMyGuestDraftsQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IUsersApi usersApi,
  IDrafterRepository drafterRepository
) : IQueryHandler<GetMyDraftsQuery, GetMyGuestDraftsResponse>
{
  // DraftStatus.cs values: Created=0, InProgress=1, Paused=2, Completed=3, Cancelled=4.
  private const int StatusCreated = 0;
  private const int StatusInProgress = 1;
  private const int StatusPaused = 2;
  private const int StatusCompleted = 3;
  private const int StatusCancelled = 4;

  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IDrafterRepository _drafterRepository = drafterRepository;

  public async Task<Result<GetMyGuestDraftsResponse>> Handle(
    GetMyDraftsQuery request,
    CancellationToken cancellationToken
  )
  {
    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure<GetMyGuestDraftsResponse>(
        UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId)
      );
    }

    // Nullable by design, same as SearchDraftsQueryHandler — a caller who's
    // never registered as a Drafter can still own drafts (ownership is
    // UserId-based); the participant half of the WHERE clause below just
    // never matches anything when this is null.
    var callerDrafter = await _drafterRepository.GetByUserIdAsync(caller.UserId, cancellationToken);

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // Single query, no COUNT, no paging — this is "everything that's mine",
    // not a search result page. Bucketing happens in C# below.
    const string sql = $"""
      SELECT
        gd.public_id         AS {nameof(DraftRow.PublicId)},
        gd.title             AS {nameof(DraftRow.Title)},
        gd.guest_draft_type   AS {nameof(DraftRow.Type)},
        gd.guest_draft_status AS {nameof(DraftRow.Status)},
        gd.scheduled_for_utc  AS {nameof(DraftRow.ScheduledForUtc)},
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
      ORDER BY gd.scheduled_for_utc ASC NULLS LAST, gd.title ASC
      """;

    var rows = (
      await connection.QueryAsync<DraftRow>(
        new CommandDefinition(
          sql,
          new { CallerUserId = caller.UserId, CallerDrafterId = callerDrafter?.Id.Value },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var items = rows.Select(r =>
        (
          Row: r,
          Summary: new MyGuestDraftSummary
          {
            PublicId = r.PublicId,
            Title = r.Title,
            Type = DraftType.FromValue(r.Type).Name,
            Status = DraftStatus.FromValue(r.Status).Name,
            ScheduledForUtc = r.ScheduledForUtc,
            IsOwner = r.IsOwner,
          }
        )
      )
      .ToList();

    var upcoming = items
      .Where(i => i.Row.Status is StatusCreated or StatusPaused)
      .Select(i => i.Summary)
      .ToList();
    var inProgress = items
      .Where(i => i.Row.Status == StatusInProgress)
      .Select(i => i.Summary)
      .ToList();
    var completed = items
      .Where(i => i.Row.Status is StatusCompleted or StatusCancelled)
      .Select(i => i.Summary)
      .ToList();

    return Result.Success(
      new GetMyGuestDraftsResponse
      {
        Upcoming = upcoming,
        InProgress = inProgress,
        Completed = completed,
      }
    );
  }

  private sealed record DraftRow
  {
    public string PublicId { get; init; } = default!;
    public string Title { get; init; } = default!;
    public int Type { get; init; } = default!;
    public int Status { get; init; } = default!;
    public DateTime? ScheduledForUtc { get; init; } = default!;
    public bool IsOwner { get; init; } = default!;
  }
}
