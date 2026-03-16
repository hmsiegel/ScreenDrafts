namespace ScreenDrafts.Modules.Drafts.Features;

internal static class DraftsAuth
{
  internal static class Roles
  {
    internal const string Admin = "Administrator";
    internal const string SuperAdmin = "SuperAdministrator";
  }
  internal static class Permissions
  {
    // Drafts
    internal const string DraftRead = "draft:read";
    internal const string DraftsList = "draft:list";
    internal const string DraftCreate = "draft:create";
    internal const string DraftUpdate = "draft:update";
    internal const string DraftDelete = "draft:delete"; // only if allowed

    // Draft Boards
    internal const string DraftBoardRead = "draft-board:read";
    internal const string DraftBoardList = "draft-board:list";
    internal const string DraftBoardCreate = "draft-board:create";
    internal const string DraftBoardUpdate = "draft-board:update";
    internal const string DraftBoardDelete = "draft-board:delete";

    // Draft Pools
    internal const string DraftPoolCreate = "draft-pool:create";
    internal const string DraftPoolUpdate = "draft-pool:update";
    internal const string DraftPoolRead = "draft-pool:read";

    // Candidate Lists
    internal const string CandidateListRead = "candidate-list:read";
    internal const string CandidateListList = "candidate-list:list";
    internal const string CandidateListCreate = "candidate-list:create";
    internal const string CandidateListUpdate = "candidate-list:update";
    internal const string CandidateListDelete = "candidate-list:delete";

    // Draft Parts
    internal const string DraftPartRead = "draft-part:read";
    internal const string DraftPartList = "draft-part:list";
    internal const string DraftPartCreate = "draft-part:create";
    internal const string DraftPartUpdate = "draft-part:update";
    internal const string DraftPartStatus = "draft-part:set-status"; // start/pause/continue/complete

    // Picks (part-scoped, but still “pick” as resource)
    internal const string PickCreate = "pick:create";        // add pick
    internal const string PickUpdate = "pick:update";
    internal const string PickUndo = "pick:undo";        // only if allowed
    internal const string PickVeto = "pick:veto";
    internal const string PickVetoOverride = "pick:veto-override";
    internal const string PickCommissionerOv = "pick:commissioner-override";

    // People + roles
    internal const string PersonRead = "person:read";
    internal const string PersonList = "person:list";
    internal const string PersonCreate = "person:create";
    internal const string PersonUpdate = "person:update";
    internal const string PersonSearch = "person:search";

    internal const string DrafterRead = "drafters:read";
    internal const string DrafterList = "drafters:list";
    internal const string DrafterCreate = "drafters:create";
    internal const string DrafterUpdate = "drafters:update";

    internal const string HostRead = "hosts:read";
    internal const string HostList = "hosts:list";
    internal const string HostCreate = "hosts:create";
    internal const string HostUpdate = "hosts:update";

    // Teams
    internal const string DrafterTeamRead = "drafter-team:read";
    internal const string DrafterTeamList = "drafter-team:list";
    internal const string DrafterTeamCreate = "drafter-team:create";
    internal const string DrafterTeamUpdate = "drafter-team:update";
    internal const string DrafterTeamMembers = "drafter-team:members"; // add/remove member

    // Game Boards (if still a thing)
    internal const string GameBoardCreate = "game-board:create";
    internal const string GameBoardUpdate = "game-board:update";

    // Reference data
    internal const string SeriesRead = "series:read";
    internal const string SeriesList = "series:list";
    internal const string SeriesCreate = "series:create";
    internal const string SeriesUpdate = "series:update";
    internal const string SeriesDelete = "series:delete"; // if allowed

    internal const string CategoryRead = "category:read";
    internal const string CategoryList = "category:list";
    internal const string CategoryCreate = "category:create";
    internal const string CategoryUpdate = "category:update";
    internal const string CategoryDelete = "category:delete";

    internal const string CampaignRead = "campaign:read";
    internal const string CampaignList = "campaign:list";
    internal const string CampaignCreate = "campaign:create";
    internal const string CampaignUpdate = "campaign:update";
    internal const string CampaignDelete = "campaign:delete";
    internal const string CampaignRestore = "campaign:restore";

    internal const string DraftReadPatreon = "draft:read-patreon";
    internal const string PatreonSearch = "patreon:search";
  }
}
