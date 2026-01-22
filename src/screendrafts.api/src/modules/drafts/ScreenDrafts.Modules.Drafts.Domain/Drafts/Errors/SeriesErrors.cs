namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class SeriesErrors
{
  public static readonly SDError SeriesNameIsRequired = SDError.Problem(
    "Series.SeriesNameIsRequired",
    "Series name is required.");

  
  public static SDError SeriesNotFound(Guid seriesId) =>
    SDError.NotFound(
      "Series.SeriesNotFound",
      $"Series with id {seriesId} was not found.");

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "Series.SeriesNotFound",
      $"Series with public id '{publicId}' was not found.");

  public static SDError SeriesIdIsInvalid(string? seriesPublicId) =>
    SDError.Problem(
      "Series.SeriesIdIsInvalid",
      $"Series public id '{seriesPublicId}' is invalid.");

  public static readonly SDError AllowedDraftTypesCannotBeNone = SDError.Problem(
    "Series.AllowedDraftTypesCannotBeNone",
    "Allowed draft types cannot be none.");

  public static readonly SDError RequiredDraftTypeMustBeIncludedInAllowedDraftTypes = SDError.Problem(
    "Series.RequiredDraftTypeMustBeIncludedInAllowedDraftTypes",
    "Required draft type must be included in allowed draft types.");

  public static readonly SDError DefaultDraftTypeMustBeIncludedInAllowedDraftTypes = SDError.Problem(
    "Series.DefaultDraftTypeMustBeIncludedInAllowedDraftTypes",
    "Default draft type must be included in allowed draft types.");

  public static readonly SDError DefaultDraftTypeMustBeSameAsRequiredDraftType = SDError.Problem(
    "Series.DefaultDraftTypeMustBeSameAsRequiredDraftType",
    "Default draft type must be the same as required draft type.");
}
