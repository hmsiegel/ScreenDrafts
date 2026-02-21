namespace ScreenDrafts.Modules.Drafts.Features;

internal static class DraftsOpenApi
{
  public static class Tags
  {
    public const string Drafts = "Drafts";
    public const string DraftParts = "DraftParts";
    public const string Campaigns = "Campaigns";
    public const string Series = "Series";
    public const string Teams = "Teams";
    public const string People = "People";
    public const string GameBoards = "GameBoards";
    public const string Drafters = "Drafters";
    public const string Hosts = "Hosts";
    public const string Picks = "Picks";
    public const string Trivia = "Trivia";
    public const string DraftPositions = "DraftPositions";
    public const string Categories = "Categories";
  }

  public static class Names
  {
    // Drafts
    public const string Drafts_CreateDraft = "Drafts.CreateDraft";
    public const string Drafts_GetDraftById = "Drafts.GetDraftById";
    public const string Drafts_SetCampaign = "Drafts.SetCampaign";
    public const string Drafts_RemoveCampaign = "Drafts.RemoveCampaign";
    public const string Drafts_GetDraftStatus = "Drafts.GetDraftStatus";
    public const string Drafts_SetDraftPartStatus = "Drafts.SetDraftPartStatus";
    public const string Drafts_UpdateDraft = "Drafts.UpdateDraft";

    // Draft Parts
    public const string DraftParts_RemoveHost = "DraftParts.RemoveHost";
    public const string DraftParts_AddParticipant = "DraftParts.AddParticipant";
    public const string DraftParts_AddHost = "DraftParts.AddHost";
    public const string DraftParts_PlayPick = "DraftParts.PlayPick";

    // Campaigns
    public const string Campaigns_CreateCampaign = "Campaigns.CreateCampaign";
    public const string Campaigns_EditCampaign = "Campaigns.EditCampaign";
    public const string Campaigns_ListCampaigns = "Campaigns.ListCampaigns";
    public const string Campaigns_GetCampaignById = "Campaigns.GetCampaignById";
    public const string Campaigns_DeleteCampaign = "Campaigns.DeleteCampaign";
    public const string Campaigns_RestoreCampaign = "Campaigns.RestoreCampaign";

    // Categories
    public const string Categories_CreateCategory = "Categories.CreateCategory";
    public const string Categories_EditCategory = "Categories.EditCategory";
    public const string Categories_ListCategories = "Categories.ListCategories";
    public const string Categories_GetCategoryById = "Categories.GetCategoryById";
    public const string Categories_DeleteCategory = "Categories.DeleteCategory";
    public const string Categories_RestoreCategory = "Categories.RestoreCategory";

    // Series
    public const string Series_CreateSeries = "Series.CreateSeries";
    public const string Series_EditSeries = "Series.EditSeries";
    public const string Series_ListSeries = "Series.ListSeries";
    public const string Series_GetSeriesById = "Series.GetSeriesById";
    public const string Series_DeleteSeries = "Series.DeleteSeries";
    public const string Series_GetSeriesMetadata = "Series.GetSeriesMetadata";

    // People
    public const string People_CreatePerson = "People.CreatePerson";
    public const string People_EditPerson = "People.EditPerson";
    public const string People_ListPeople = "People.ListPeople";
    public const string People_GetPersonById = "People.GetPersonById";
    public const string People_DeletePerson = "People.DeletePerson";
    public const string People_RestorePerson = "People.RestorePerson";
    public const string People_SearchPeople = "People.SearchPeople";
    public const string People_LinkUser = "People.LinkUser";

    // Drafters
    public const string Drafters_CreateDrafter = "Drafters.CreateDrafter";
    public const string Drafters_RetireDrafter = "Drafters.RetireDrafter";
    public const string Drafters_GetDrafterById = "Drafters.GetDrafterById";
    public const string Drafters_ListDrafters = "Drafters.ListDrafters";

    // Hosts
    public const string Hosts_CreateHost = "Hosts.CreateHost";
    public const string Hosts_RetireHost = "Hosts.RetireHost";
    public const string Hosts_GetHostById = "Hosts.GetHostById";
    public const string Hosts_ListHosts = "Hosts.ListHosts";
  }
}
