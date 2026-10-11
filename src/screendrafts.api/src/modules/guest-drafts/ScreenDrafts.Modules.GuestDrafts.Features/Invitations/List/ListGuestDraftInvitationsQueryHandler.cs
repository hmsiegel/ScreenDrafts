namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.List;

// ── Handler ───────────────────────────────────────────────────────────────────
internal sealed class ListGuestDraftInvitationsQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IUsersApi usersApi,
  IDateTimeProvider dateTimeProvider
) : IQueryHandler<ListGuestDraftInvitationsQuery, ListGuestDraftInvitationsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<ListGuestDraftInvitationsResponse>> Handle(
    ListGuestDraftInvitationsQuery request,
    CancellationToken cancellationToken
  )
  {
    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure<ListGuestDraftInvitationsResponse>(
        UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId)
      );
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string draftSql = $"""
      SELECT
        d.id AS {nameof(DraftRow.Id)},
        d.owner_user_id AS {nameof(DraftRow.OwnerUserId)}
      FROM guest_drafts.drafts d
      WHERE d.public_id = @PublicId
      """;

    var draft = await connection.QuerySingleOrDefaultAsync<DraftRow>(
      new CommandDefinition(
        draftSql,
        new { PublicId = request.GuestDraftPublicId },
        cancellationToken: cancellationToken
      )
    );

    if (draft is null)
    {
      return Result.Failure<ListGuestDraftInvitationsResponse>(
        DraftErrors.NotFound(request.GuestDraftPublicId)
      );
    }

    if (draft.OwnerUserId != caller.UserId)
    {
      return Result.Failure<ListGuestDraftInvitationsResponse>(
        DraftErrors.OnlyOwnerCanPerformThisAction
      );
    }

    const string invitationsSql = $"""
      SELECT
        i.public_id AS {nameof(InvitationRow.PublicId)},
        i.email AS {nameof(InvitationRow.Email)},
        i.name AS {nameof(InvitationRow.Name)},
        i.last_sent_on_utc AS {nameof(InvitationRow.LastSentOnUtc)},
        i.accepted_on_utc AS {nameof(InvitationRow.AcceptedOnUtc)}
      FROM guest_drafts.draft_invitations i
      WHERE i.draft_id = @DraftId
      ORDER BY i.created_on_utc ASC
      """;

    var rows = (
      await connection.QueryAsync<InvitationRow>(
        new CommandDefinition(
          invitationsSql,
          new { DraftId = draft.Id },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var now = _dateTimeProvider.UtcNow;

    var items = rows.Select(r =>
      {
        var pending = r.AcceptedOnUtc is null;
        var availableOn = r.LastSentOnUtc + Invitation.ResendCooldown;

        return new GuestDraftInvitationResponse
        {
          PublicId = r.PublicId,
          Email = r.Email,
          Name = r.Name,
          Status = pending ? "Pending" : "Accepted",
          SentOnUtc = r.LastSentOnUtc,
          CanResend = pending && now >= availableOn,
          ResendAvailableOnUtc = pending ? availableOn : null,
        };
      })
      .ToList();

    return Result.Success(new ListGuestDraftInvitationsResponse { Invitations = items });
  }

  private sealed record DraftRow(Guid Id, Guid OwnerUserId);

  private sealed record InvitationRow
  {
    public string PublicId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public DateTime LastSentOnUtc { get; set; } = default!;
    public DateTime? AcceptedOnUtc { get; set; } = default!;
  }
}
