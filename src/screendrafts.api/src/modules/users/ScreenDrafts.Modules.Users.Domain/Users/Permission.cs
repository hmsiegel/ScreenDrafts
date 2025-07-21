namespace ScreenDrafts.Modules.Users.Domain.Users;

public sealed class Permission(string code)
{
  public static readonly Permission GetUser = new("users:read");
  public static readonly Permission ModifyUser = new("users:update");
  public static readonly Permission GetDrafts = new("drafts:read");
  public static readonly Permission SearchDrafts = new("drafts:search");
  public static readonly Permission CreateDraft = new("drafts:create");
  public static readonly Permission ModifyDraft = new("drafts:update");
  public static readonly Permission AddPicks = new("picks:add");
  public static readonly Permission VetoPicks = new("picks:veto");
  public static readonly Permission VetoOverride = new("picks:veto-override");
  public static readonly Permission AddDrafters = new("drafters:add");
  public static readonly Permission CreateDrafters = new("drafters:create");
  public static readonly Permission RemoveDrafters = new("drafters:remove");
  public static readonly Permission ModifyDrafters = new("drafters:update");
  public static readonly Permission GetDrafters = new("drafters:read");
  public static readonly Permission GetRoles = new("roles:read");
  public static readonly Permission ModifyRoles = new("roles:update");
  public static readonly Permission GetPermissions = new("permissions:read");
  public static readonly Permission ModifyPermissions = new("permissions:update");
  public static readonly Permission AddHosts = new("hosts:add");
  public static readonly Permission CreateHosts = new("hosts:create");
  public static readonly Permission RemoveHosts = new("hosts:remove");
  public static readonly Permission ModifyHosts = new("hosts:update");
  public static readonly Permission GetHosts = new("hosts:read");
  public static readonly Permission SearchMovies = new("movies:search");
  public static readonly Permission SearchActors = new("actors:search");
  public static readonly Permission SearchCrew = new("crew:search");
  public static readonly Permission SearchGenres = new("genres:search");
  public static readonly Permission SearchStudios = new("studios:search");
  public static readonly Permission CreateGameBoard = new("game-boards:create");
  public static readonly Permission ModifyGameBoard = new("game-boards:update");
  public static readonly Permission SearchPatreonDrafts = new("patreon:search");
  public static readonly Permission CreatePeople = new("people:create");
  public static readonly Permission GetPeople = new( "people:read");
  public static readonly Permission UpdatePerson = new("people:update");
  public static readonly Permission SearchPeople = new("people:search");

  public string Code { get; } = code;
}
