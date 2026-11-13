namespace ScreenDrafts.Modules.Movies.Features.Movies.ListMediaPeople;

internal static class MediaPeopleListRoutes
{
  // Beside GET /media/people (the picker search) and /media/people/{id} (one profile).
  // The literal "browse" segment wins over {id}; person IDs are digits or "nm…", never "browse".
  internal const string Browse = "/media/people/browse";
  internal const string BrowseOperationName = "Media.ListPeople";
}
