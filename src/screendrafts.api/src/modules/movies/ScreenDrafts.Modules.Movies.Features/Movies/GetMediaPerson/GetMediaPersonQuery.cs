namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPerson;

internal sealed record GetMediaPersonQuery : IQuery<MediaPersonProfileResponse>
{
  public required string Id { get; init; }
}
