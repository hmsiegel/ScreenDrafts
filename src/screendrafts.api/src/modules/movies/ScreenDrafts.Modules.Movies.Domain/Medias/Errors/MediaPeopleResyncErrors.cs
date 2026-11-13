namespace ScreenDrafts.Modules.Movies.Domain.Medias.Errors;

public static class MediaPeopleResyncErrors
{
  public static readonly SDError AlreadyRunning = SDError.Conflict(
    "Media.PeopleResyncAlreadyRunning",
    "A people resync is already running. Check its status, or cancel it first."
  );

  public static readonly SDError InvalidLimit = SDError.Failure(
    "Media.PeopleResyncInvalidLimit",
    "The limit must be a positive number."
  );
}
