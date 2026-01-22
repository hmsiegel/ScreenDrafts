
namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
public static class CategoryErrors
{
  public static readonly SDError CategoryNameIsRequired = SDError.Problem(
    "Categories.CategoryNameIsRequired",
    "Category name is required.");

  public static readonly SDError CategoryDescriptionIsRequired = SDError.Problem(
    "Categories.CategoryDescriptionIsRequired",
    "Category description is required.");

  public static SDError DuplicateName(string name) =>
    SDError.Conflict(
      "Categories.DuplicateName",
      $"A category with the name '{name}' already exists.");

  public static SDError NotFound(Guid categoryId) =>
    SDError.NotFound(
      "Categories.NotFound",
      $"Category with id {categoryId} was not found.");

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "Categories.NotFound",
      $"Category with public id {publicId} was not found.");

  public static SDError CategoryAlreadyAdded(Guid categoryId) =>
    SDError.Conflict(
      "Categories.CategoryAlreadyAdded",
      $"Category with id {categoryId} has already been added to the draft.");

  public static SDError CannotRemoveACategoryThatIsNotAdded(Guid categoryId) =>
    SDError.Problem(
      "Categories.CannotRemoveACategoryThatIsNotAdded",
      $"Cannot remove category with id {categoryId} because it has not been added to the list.");

  public static SDError CategoryNameTooLong(int length) =>
    SDError.Problem(
      "Categories.CategoryNameTooLong",
      $"Category name must not exceed {length} characters.");

  public static SDError CategoryDescriptionTooLong(int length) =>
    SDError.Problem(
      "Categories.CategoryDescriptionTooLong",
      $"Category description must not exceed {length} characters.");

  public static SDError CategoryStillInUse(Guid categoryId) =>
    SDError.Conflict(
      "Categories.CategoryInUse",
      $"Category with id {categoryId} is still in use by one or more drafts and cannot be removed.");

  public static SDError DeletionFailed(string publicId) =>
    SDError.Problem(
      "Categories.DeletionFailed",
      $"Deletion of category with public id {publicId} failed.");

  public static SDError OneOrMoreCategoryIdsAreInvalid(IReadOnlyList<string> publicCategoryIds) =>
    SDError.Problem(
      "Categories.OneOrMoreCategoryIdsAreInvalid",
      $"One or more category IDs are invalid: {string.Join(", ", publicCategoryIds)}.");
}
