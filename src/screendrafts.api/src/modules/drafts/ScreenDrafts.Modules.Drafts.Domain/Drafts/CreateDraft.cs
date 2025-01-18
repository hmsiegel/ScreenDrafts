namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public static class CreateDraft
{
  public static void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("/drafts", async (DraftsDbContext dbContext, DraftRequest request) =>
        {
          var draft = new Draft
          {
            Id = Guid.NewGuid(),
            Title = request.Title,
            DraftType = DraftType.FromValue(request.DraftType),
            NumberOfDrafters = request.NumberOfDrafters,
            NumberOfCommissioners = request.NumberOfCommissioners,
            NumberOfMovies = request.NumberOfMovies
          };

          dbContext.Drafts.Add(draft);
          await dbContext.SaveChangesAsync();
          return Results.Ok(draft.Id);
        })
      .WithTags(Tags.Drafts);
  }
}

