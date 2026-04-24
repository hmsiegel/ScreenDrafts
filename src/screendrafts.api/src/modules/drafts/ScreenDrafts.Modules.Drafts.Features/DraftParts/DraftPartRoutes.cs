namespace ScreenDrafts.Modules.Drafts.Features.DraftParts;

internal static class DraftPartRoutes
{
  // Core
  public const string Base = "/drafts/{draftId}/parts";
  public const string ById = "/draft-parts/{draftPartId}";

  // Status
  public const string Status = ById + "/status";

  // Releases
  public const string Releases = ById + "/releases";
  public const string RemoveReleaseDate = ById + "/releases/{releaseId}";
  public const string Episode = ById + "/episode";

  // Hosts
  public const string Hosts = ById + "/hosts";
  public const string RemoveHost = ById + "/hosts/{hostId}";
  public const string SetPrimaryHost = ById + "/hosts/{hostId}/primary";

  // Participants
  public const string Participants = ById + "/participants";
  public const string SetCommunityParticipant = ById + "/participants/community";
  public const string RemoveCommunityParticipant = ById + "/participants/community/{communityId}";
  public const string DraftPositions = ById + "/positions";
  public const string ParticipantDraftPosition = ById + "/positions/{positionId}/participant";
  public const string AssignTriviaResults = ById + "/trivia-results";

  // Picks
  public const string Picks = ById + "/picks";
  public const string UndoPick = ById + "/picks/{playOrder}";

  // Vetos and Veto Overrides
  public const string ApplyVeto = ById + "/picks/{playOrder}/veto";
  public const string ApplyVetoOverride = ById + "/veto-override/{pickId}";
  public const string ApplyCommissionerOverride = ById + "/commissioner-override/{pickId}";

  public const string SetCommunityLimits = ById + "/community-limits";

  // Candidate Lists
  public const string CandidateList = ById + "/candidate-list";
  public const string CandidateListBulkAdd = ById + "/candidate-list/bulk";
  public const string CandidateListEntry = ById + "/candidate-list/{tmdbid}";

  // Speed Draft Sub-Drafts
  public const string SubDrafts = ById + "/sub-drafts";
  public const string SubDraftById = ById + "/sub-drafts/{subDraftId}";
  public const string SubDraftSubject = ById + "/sub-drafts/{subDraftId}/subject";
  public const string SubDraftTrivia = ById + "/sub-drafts/{subDraftId}/trivia-results";
  public const string SubDraftPicks = ById + "/sub-drafts/{subDraftId}/picks";
  public const string SubDraftVeto = ById + "/sub-drafts/{subDraftId}/picks/{playOrder}/veto";
  public const string SubDraftAdvance = ById + "/sub-drafts/{subDraftId}/advance";

  // Zoom Recordings
  public const string ZoomSession = ById + "/zoom-session";
  public const string ZoomSessionToken = ZoomSession + "/token";
  public const string StartZoomSessionRecording = ZoomSession + "/recording/start";
  public const string StopZoomSessionRecording = ZoomSession + "/recording/stop";
} 
