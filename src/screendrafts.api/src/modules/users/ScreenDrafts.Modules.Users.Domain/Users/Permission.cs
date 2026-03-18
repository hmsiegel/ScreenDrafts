namespace ScreenDrafts.Modules.Users.Domain.Users;

public sealed class Permission(string code)
{
  // Users
  public static readonly Permission GetUser = new("users:read");
  public static readonly Permission ModifyUser = new("users:update");

  // Roles & Permissions (admin)
  public static readonly Permission GetRoles = new("roles:read");
  public static readonly Permission ModifyRoles = new("roles:update");
  public static readonly Permission GetPermissions = new("permissions:read");
  public static readonly Permission ModifyPermissions = new("permissions:update");

  // Drafts
  public static readonly Permission GetDrafts = new("drafts:read");
  public static readonly Permission ListDrafts = new("drafts:list");
  public static readonly Permission CreateDraft = new("drafts:create");
  public static readonly Permission ModifyDraft = new("drafts:update");
  public static readonly Permission DeleteDraft = new("drafts:delete");
  public static readonly Permission SearchDrafts = new("drafts:search");
  public static readonly Permission ReadPatreonDrafts = new("drafts:read-patreon");
  public static readonly Permission SearchPatreonDrafts = new("patreon:search");

  // Draft Parts
  public static readonly Permission ReadDraftPart = new("draft-parts:read");
  public static readonly Permission ListDraftParts = new("draft-parts:list");
  public static readonly Permission CreateDraftPart = new("draft-parts:create");
  public static readonly Permission UpdateDraftPart = new("draft-parts:update");
  public static readonly Permission SetDraftPartStatus = new("draft-parts:set-status");

  // Draft Boards
  public static readonly Permission ReadDraftBoard = new("draft-boards:read");
  public static readonly Permission ListDraftBoards = new("draft-boards:list");
  public static readonly Permission CreateDraftBoard = new("draft-boards:create");
  public static readonly Permission UpdateDraftBoard = new("draft-boards:update");
  public static readonly Permission DeleteDraftBoard = new("draft-boards:delete");

  // Draft Pools
  public static readonly Permission CreateDraftPool = new("draft-pools:create");
  public static readonly Permission UpdateDraftPool = new("draft-pools:update");
  public static readonly Permission ReadDraftPool = new("draft-pools:read");

  // Picks
  public static readonly Permission AddPicks = new("picks:add");
  public static readonly Permission CreatePick = new("picks:create");
  public static readonly Permission UpdatePick = new("picks:update");
  public static readonly Permission UndoPick = new("picks:undo");
  public static readonly Permission VetoPicks = new("picks:veto");
  public static readonly Permission VetoOverride = new("picks:veto-override");
  public static readonly Permission CommissionerOverride = new("picks:commissioner-override");

  // People
  public static readonly Permission CreatePeople = new("people:create");
  public static readonly Permission GetPeople = new("people:read");
  public static readonly Permission ListPeople = new("people:list");
  public static readonly Permission UpdatePerson = new("people:update");
  public static readonly Permission SearchPeople = new("people:search");

  // Drafters
  public static readonly Permission AddDrafters = new("drafters:add");
  public static readonly Permission CreateDrafters = new("drafters:create");
  public static readonly Permission RemoveDrafters = new("drafters:remove");
  public static readonly Permission ModifyDrafters = new("drafters:update");
  public static readonly Permission GetDrafters = new("drafters:read");
  public static readonly Permission ListDrafters = new("drafters:list");

  // Hosts
  public static readonly Permission AddHosts = new("hosts:add");
  public static readonly Permission CreateHosts = new("hosts:create");
  public static readonly Permission RemoveHosts = new("hosts:remove");
  public static readonly Permission ModifyHosts = new("hosts:update");
  public static readonly Permission GetHosts = new("hosts:read");
  public static readonly Permission ListHosts = new("hosts:list");

  // Drafter Teams
  public static readonly Permission ReadDrafterTeam = new("drafter-teams:read");
  public static readonly Permission ListDrafterTeams = new("drafter-teams:list");
  public static readonly Permission CreateDrafterTeam = new("drafter-teams:create");
  public static readonly Permission UpdateDrafterTeam = new("drafter-teams:update");
  public static readonly Permission ManageDrafterTeamMembers = new("drafter-teams:members");

  // Game Boards
  public static readonly Permission CreateGameBoard = new("game-boards:create");
  public static readonly Permission ModifyGameBoard = new("game-boards:update");

  // Movies
  public static readonly Permission CreateMovies = new("movies:create");
  public static readonly Permission GetMovies = new("movies:read");
  public static readonly Permission SearchMovies = new("movies:search");
  public static readonly Permission SearchActors = new("actors:search");
  public static readonly Permission SearchCrew = new("crew:search");
  public static readonly Permission SearchGenres = new("genres:search");
  public static readonly Permission SearchStudios = new("studios:search");

  // Series
  public static readonly Permission ReadSeries = new("series:read");
  public static readonly Permission ListSeries = new("series:list");
  public static readonly Permission CreateSeries = new("series:create");
  public static readonly Permission UpdateSeries = new("series:update");
  public static readonly Permission DeleteSeries = new("series:delete");

  // Categories
  public static readonly Permission CreateCategories = new("categories:create");
  public static readonly Permission UpdateCategories = new("categories:update");
  public static readonly Permission GetCategories = new("categories:read");
  public static readonly Permission SearchCategories = new("categories:search");
  public static readonly Permission DeleteCategories = new("categories:delete");

  // Campaigns
  public static readonly Permission CreateCampaign = new("campaigns:create");
  public static readonly Permission UpdateCampaign = new("campaigns:update");
  public static readonly Permission ReadCampaign = new("campaigns:read");
  public static readonly Permission ListCampaigns = new("campaigns:list");
  public static readonly Permission DeleteCampaign = new("campaigns:delete");
  public static readonly Permission RestoreCampaign = new("campaigns:restore");

  // Candidate Lists
  public static readonly Permission ReadCandidateList = new("candidate-lists:read");
  public static readonly Permission ListCandidateLists = new("candidate-lists:list");
  public static readonly Permission CreateCandidateList = new("candidate-lists:create");
  public static readonly Permission UpdateCandidateList = new("candidate-lists:update");
  public static readonly Permission DeleteCandidateList = new("candidate-lists:delete");

  public string Code { get; } = code;
}
