namespace ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

internal static class MediaPeopleResyncRoutes
{
  // Literal two-segment paths: they win over GET /media/{publicId}, as /media/search does.
  internal const string Base = "/media/people-resync";
  internal const string Cancel = Base + "/cancel";

  internal const string StartName = "Media.StartPeopleResync";
  internal const string StatusName = "Media.GetPeopleResyncStatus";
  internal const string CancelName = "Media.CancelPeopleResync";
}
