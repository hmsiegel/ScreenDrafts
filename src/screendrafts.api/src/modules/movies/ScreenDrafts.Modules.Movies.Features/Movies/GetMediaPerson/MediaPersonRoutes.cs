namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPerson;

internal static class MediaPersonRoutes
{
  // Sits beside GET /media/people (the person search) and /media/{publicId}.
  // The literal "people" segment wins over {publicId} in route matching.
  internal const string GetPerson = "/media/people/{id}";
  internal const string GetPersonOperationName = "Media.GetPerson";
}
