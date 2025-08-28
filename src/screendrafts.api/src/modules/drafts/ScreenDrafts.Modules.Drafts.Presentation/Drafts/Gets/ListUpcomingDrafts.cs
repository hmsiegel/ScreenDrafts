using ScreenDrafts.Common.Infrastructure.Authentication;

namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Gets;

internal sealed class ListUpcomingDrafts(ISender sender, IUsersApi usersApi) : EndpointWithoutRequest<List<UpcomingDraftResponse>>
{
  private readonly ISender _sender = sender;
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Get("/drafts/upcoming");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithName(nameof(ListUpcomingDrafts))
      .WithDescription("Get all upcoming drafts");
    });

    Policies(Presentation.Permissions.SearchDrafts);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var user = await _usersApi.GetUserByIdAsync(User.GetUserId(), ct);

    if (user is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var userRoles = await _usersApi.GetUserRolesAsync(user.UserId, ct);
    var isAdmin = userRoles.Contains(Presentation.Roles.Admin, StringComparer.OrdinalIgnoreCase) || userRoles.Contains(Presentation.Roles.SuperAdmin, StringComparer.OrdinalIgnoreCase);
    var canViewPatreon = User.HasClaim(p => p.Type == "permission" && p.Value == Presentation.Permissions.PatronSearchDrafts);

    var query = new ListUpcomingDraftsQuery(
      IsPatreonOnly: canViewPatreon,
      UserId: user.UserId,
      IsAdmin: isAdmin);
    var result = await _sender.Send(query, ct);

    if (result.IsFailure)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }

    var response = result.Value
      .Select(draft => new UpcomingDraftResponse
      {
        Id = draft.Id,
        Title = draft.Title,
        DraftStatus = draft.DraftStatus,
        ReleaseDates = [.. draft.ReleaseDates.Select(d => DateOnly.FromDateTime(d))],
        Capabilities = draft.Capabilities,
      })
      .ToList();

    await Send.OkAsync(response, ct);
  }
}

internal sealed class ListUpcomingDraftsSummary : Summary<ListUpcomingDrafts>
{
  public ListUpcomingDraftsSummary()
  {
    Summary = "Get all upcoming drafts";
    Description = "Get all upcoming drafts. This endpoint returns a list of all upcoming drafts.";
    Response<List<UpcomingDraftDto>>(StatusCodes.Status200OK, "List of all upcoming drafts.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}

public sealed record UpcomingDraftResponse
{
  public Guid Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public DateOnly[] ReleaseDates { get; init; } = [];
  public int DraftStatus { get; init; }
  public string DisplayNextReleaseDate => ReleaseDates.Length == 0
    ? "TBD"
    : ReleaseDates.Min().ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
  public DraftUserCapabilities Capabilities { get; init; } = default!;
}
