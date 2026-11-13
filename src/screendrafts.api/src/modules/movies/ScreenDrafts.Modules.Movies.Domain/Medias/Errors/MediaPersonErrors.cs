namespace ScreenDrafts.Modules.Movies.Domain.Medias.Errors;

public static class MediaPersonErrors
{
  public static SDError NotFound(string id) =>
    SDError.NotFound("Media.PersonNotFound", $"No person with the ID '{id}' is credited in the Vault.");
}
