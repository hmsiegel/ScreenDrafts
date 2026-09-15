namespace ScreenDrafts.Modules.Users.Features;

internal static class UsersOpenApi
{
  public static class Tags
  {
    public const string Users = "Users";
    public const string Admin = "Admin";
  }

  public static class Names
  {
    // Users
    public const string Users_RegisterUser = "Users.RegisterUser";
    public const string Users_GetUserById = "Users.GetUserById";
    public const string Users_UpdateUserProfile = "Users.UpdateUserProfile";
    public const string Users_GetUserProfile = "Users.GetUserProfile";
    public const string Users_GetUsersProfiles = "Users.GetUsersProfiles";
    public const string Users_RegisterSocialUser = "Users.RegisterSocialUser";
    public const string Users_UpdateUserPassword = "Users.UpdateUserPassword";
    public const string Users_GenerateEmailBootstrapTokens = "Users.GenerateEmailBootstrapTokens";
    public const string Users_ValidateEmailBootstrapToken = "Users.ValidateEmailBootstrapToken";
    public const string Users_ClaimEmailBootstrapToken = "Users.ClaimEmailBootstrapToken";
    public const string Users_RequestEmailChange = "Users.RequestEmailChange";
    public const string Users_ConfirmEmailChange = "Users.ConfirmEmailChange";
    public const string Users_ListEmailBootstrapCandidates = "Users.ListEmailBootstrapCandidates";
  }
}
