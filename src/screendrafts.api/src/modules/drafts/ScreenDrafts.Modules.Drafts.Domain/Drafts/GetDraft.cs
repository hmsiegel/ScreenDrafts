namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public static class GetDraft
{
  public static void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("drafts/{id}", async (Guid id, DraftsDbContext context) =>
    {
      var draft = await context.Drafts
      .Where(d => d.Id == id)
      .Select(d => new DraftResponse(
        d.Id,
        d.Title,
        d.DraftType,
        d.NumberOfDrafters,
        d.NumberOfCommissioners,
        d.NumberOfMovies))
      .SingleOrDefaultAsync();

      return draft is null ? Results.NotFound() : Results.Ok(draft);
    })
      .WithTags(Tags.Drafts);
  }
}
