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
    internal const string DraftRead = "drafts:read";
    internal const string DraftList = "drafts:list";
    internal const string DraftCreate = "drafts:create";
    internal const string DraftUpdate = "drafts:update";
    internal const string DraftDelete = "drafts:delete";
    internal const string DraftSearch = "drafts:search";
    internal const string DraftReadPatreon = "drafts:read-patreon";

    // Draft Parts
    internal const string DraftPartRead = "draft-parts:read";
    internal const string DraftPartList = "draft-parts:list";
    internal const string DraftPartCreate = "draft-parts:create";
    internal const string DraftPartUpdate = "draft-parts:update";
    internal const string DraftPartStatus = "draft-parts:set-status";

    // Draft Boards
    internal const string DraftBoardRead = "draft-boards:read";
    internal const string DraftBoardList = "draft-boards:list";
    internal const string DraftBoardCreate = "draft-boards:create";
    internal const string DraftBoardUpdate = "draft-boards:update";
    internal const string DraftBoardDelete = "draft-boards:delete";

    // Draft Pools
    internal const string DraftPoolCreate = "draft-pools:create";
    internal const string DraftPoolUpdate = "draft-pools:update";
    internal const string DraftPoolRead = "draft-pools:read";

    // Picks
    internal const string PickAdd = "picks:add";
    internal const string PickCreate = "picks:create";
    internal const string PickUpdate = "picks:update";
    internal const string PickUndo = "picks:undo";
    internal const string PickVeto = "picks:veto";
    internal const string PickVetoOverride = "picks:veto-override";
    internal const string PickCommissionerOverride = "picks:commissioner-override";

    // People
    internal const string PersonCreate = "people:create";
    internal const string PersonRead = "people:read";
    internal const string PersonList = "people:list";
    internal const string PersonUpdate = "people:update";
    internal const string PersonSearch = "people:search";

    // Drafters
    internal const string DrafterAdd = "drafters:add";
    internal const string DrafterCreate = "drafters:create";
    internal const string DrafterRemove = "drafters:remove";
    internal const string DrafterUpdate = "drafters:update";
    internal const string DrafterRead = "drafters:read";
    internal const string DrafterList = "drafters:list";

    // Hosts
    internal const string HostAdd = "hosts:add";
    internal const string HostCreate = "hosts:create";
    internal const string HostRemove = "hosts:remove";
    internal const string HostUpdate = "hosts:update";
    internal const string HostRead = "hosts:read";
    internal const string HostList = "hosts:list";

    // Drafter Teams
    internal const string DrafterTeamRead = "drafter-teams:read";
    internal const string DrafterTeamList = "drafter-teams:list";
    internal const string DrafterTeamCreate = "drafter-teams:create";
    internal const string DrafterTeamUpdate = "drafter-teams:update";
    internal const string DrafterTeamMembers = "drafter-teams:members";

    // Game Boards
    internal const string GameBoardCreate = "game-boards:create";
    internal const string GameBoardUpdate = "game-boards:update";

    // Series
    internal const string SeriesRead = "series:read";
    internal const string SeriesList = "series:list";
    internal const string SeriesCreate = "series:create";
    internal const string SeriesUpdate = "series:update";
    internal const string SeriesDelete = "series:delete";

    // Categories
    internal const string CategoryCreate = "categories:create";
    internal const string CategoryUpdate = "categories:update";
    internal const string CategoryRead = "categories:read";
    internal const string CategoryList = "categories:list";
    internal const string CategorySearch = "categories:search";
    internal const string CategoryDelete = "categories:delete";

    // Campaigns
    internal const string CampaignCreate = "campaigns:create";
    internal const string CampaignUpdate = "campaigns:update";
    internal const string CampaignRead = "campaigns:read";
    internal const string CampaignList = "campaigns:list";
    internal const string CampaignDelete = "campaigns:delete";
    internal const string CampaignRestore = "campaigns:restore";

    // Candidate Lists
    internal const string CandidateListRead = "candidate-lists:read";
    internal const string CandidateListList = "candidate-lists:list";
    internal const string CandidateListCreate = "candidate-lists:create";
    internal const string CandidateListUpdate = "candidate-lists:update";
    internal const string CandidateListDelete = "candidate-lists:delete";

    // Patreon
    internal const string PatreonSearch = "patreon:search";

    // Speed Drafts
    internal const string SubDraftRead = "sub-drafts:read";
    internal const string SubDraftCreate = "sub-drafts:create";
    internal const string SubDraftUpdate = "sub-drafts:update";
  }
}
