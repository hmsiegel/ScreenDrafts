namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public static class ListDrafts
{
  public static void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("/drafts", async (DraftsDbContext context) =>
    {
      var drafts = await context.Drafts.ToListAsync();

      return Results.Ok(drafts);
    })
      .WithTags("Drafts");
  }

}
