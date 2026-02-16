namespace ScreenDrafts.Modules.Drafts.Features.People.Get;

internal sealed record GetPersonRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
}

