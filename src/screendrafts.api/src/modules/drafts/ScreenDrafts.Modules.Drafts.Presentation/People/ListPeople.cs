namespace ScreenDrafts.Modules.Drafts.Presentation.People;

internal sealed class ListPeople(ISender sender) : Endpoint<ListPeopleRequest, Result<PagedResult<PersonResponse>>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/people");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.People)
       .WithDescription("Get all people")
       .WithName(nameof(ListPeople));
    });
    Policies(Presentation.Permissions.SearchPeople);
  }

  public override async Task HandleAsync(ListPeopleRequest req, CancellationToken ct)
  {
    var query = new ListPeopleQuery(
      Page: req.Page,
      PageSize: req.PageSize,
      FirstName: req.FirstName,
      LastName: req.LastName,
      DisplayName: req.DisplayName,
      IsDrafter: req.IsDrafter,
      IsHost: req.IsHost,
      Sort: req.Sort,
      Dir: req.Dir,
      Q: req.Q);

    var result = await _sender.Send(query, ct);

    if (result.IsSuccess && result.Value.Items.Count != 0)
    {
      await SendOkAsync(result, ct);
    }
    else
    {
      await SendNoContentAsync(ct);
    }
  }
}

public sealed record ListPeopleRequest(
  int Page,
  int PageSize,
  string? FirstName = null,
  string? LastName = null,
  string? DisplayName = null,
  bool? IsDrafter = null,
  bool? IsHost = null,
  string? Sort = null,
  string? Dir = null,
  string? Q = null);

internal sealed class ListPeopleSummary : Summary<ListPeople>
{
  public ListPeopleSummary()
  {
    Description = "Returns a list of all of the people, both drafters and hosts.";
    Summary = "Get all people";
    Response<PagedResult<PersonResponse>>(200, "A paginated list of people.", "application/json");
    Response(204, "No content.", "application/json");
    Response(400, "Bad request.", "application/json");
    Response(500, "Internal server error.", "application/json");
    Response(401, "Unauthorized.", "application/json");
  }
}
