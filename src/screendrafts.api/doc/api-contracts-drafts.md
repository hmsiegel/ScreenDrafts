# Drafts Module API Contract (PublicId-first)

This document is a one-page  for refactoring the Drafts module into Vertical Slice architecture.

## PublicId Rules

- **All route IDs are PublicIds (string)** (e.g., NanoID, or prefixed IDs like `d_...`, `dp_...`).
- Internal database IDs (UUID/int) must not appear in URLs.
- In this doc, `{draftId}`, `{draftPartId}`, `{hostId}`, etc. mean **PublicId strings**, not DB IDs.

### Recommended response shape for created resources
- On create, return `201 Created` with `Location` header pointing at the new resource.
- Response body should include at least `{ publicId: "..." }` (and optionally the full resource).

---

## Reference Data

### Series
- **POST** `/series` — CreateSeries
- **GET** `/series` — SearchSeries
- **GET** `/series/{publicId}` — GetSeries
- **PATCH** `/series/{publicId}` — UpdateSeries
- **DELETE** `/series/{publicId}` — DeleteSeries
- **POST** `/series/{publicId}/restore` — RestoreSeries

### Campaigns
- **POST** `/campaigns` — CreateCampaign
- **GET** `/campaigns` — GetCampaigns
- **GET** `/campaigns/{publicId}` — GetCampaign
- **PATCH** `/campaigns/{publicId}` — UpdateCampaign
- **DELETE** `/campaigns/{publicId}` — DeleteCampaign
- **POST** `/campaigns/{publicId}/restore` — RestoreCampaign

### Categories
- **POST** `/categories` — CreateCategory
- **GET** `/categories` — GetCategories
- **GET** `/categories/{publicId}` — GetCategory
- **PATCH** `/categories/{publicId}` — UpdateCategory
- **DELETE** `/categories/{publicId}` — DeleteCategory
- **POST** `/categories/{publicId}/restore` — RestoreCategory

---

## People + Roles (Drafts domain)

> Users are owned by the Users module. Drafts owns People and role entities (Drafter/Host) that may link to a User.

### People
- **POST** `/people` — CreatePerson (optionally link to User publicId)
- **GET** `/people/{personId}` — GetPerson (optional)
- **GET** `/people` — SearchPeople (optional)

### Drafters (role)
- **POST** `/drafters` — CreateDrafter (promote Person → Drafter)  
  Body: `{ "personId": "p_..." }`

### Hosts (role)
- **POST** `/hosts` — CreateHost (promote Person → Host)  
  Body: `{ "personId": "p_..." }`

### DrafterTeams
- **POST** `/drafter-teams` — CreateDrafterTeam
- **GET** `/drafter-teams/{drafterTeamId}` — GetDrafterTeam (optional)
- **GET** `/drafter-teams` — SearchDrafterTeams (optional)

### Team membership
- **POST** `/drafter-teams/{drafterTeamId}/members` — AddDrafterToTeam  
  Body: `{ "drafterId": "dr_..." }`
- **DELETE** `/drafter-teams/{drafterTeamId}/members/{drafterId}` — RemoveDrafterFromTeam (optional)

---

## Drafts (draft-wide)

- **POST** `/drafts` — CreateDraft
- **GET** `/drafts/{draftId}` — GetDraft
- **GET** `/drafts` — SearchDrafts
- **PUT** `/drafts/{draftId}` — UpdateDraftMetadata (includes name, description, soft delete)
- **PUT** `/drafts/{draftId}/categories` — SetDraftCategories (replace list) 
- **POST** `/drafts/{draftId}/campaign` — SetDraftCampaign
- **DELETE** `/drafts/{draftId}/campaign` — ClearDraftCampaign

---

## DraftParts (part-scoped gameplay)

### DraftParts core
- **POST** `/drafts/{draftId}/parts` — CreateDraftPart
- **GET** `/draft-parts/{draftPartId}` — GetDraftPart
- **GET** `/drafts/{draftId}/parts` — SearchDraftParts (optional/admin)
- **PUT** `/draft-parts/{draftPartId}` — UpdateDraftPartMetadata

### DraftPart status (single endpoint)
- **PUT** `/draft-parts/{draftPartId}/status` — SetDraftPartStatus  
  Body: `{ "status": "InProgress" }`

---

## Releases (DraftPart scoped)

- **POST** `/draft-parts/{draftPartId}/releases` — AddReleaseDate
- **DELETE** `/draft-parts/{draftPartId}/releases/{releaseId}` — RemoveReleaseDate
- **PUT** `/draft-parts/{draftPartId}/episode` — SetEpisodeNumber
- **POST** `/draft-parts/{draftPartId}/publish` — PublishRelease (if applicable)

---

## Hosts (DraftPart scoped)

- **POST** `/draft-parts/{draftPartId}/hosts` — AddHost  
  Body: `{ "hostId": "h_...", "role": "Primary|CoHost" }`
- **DELETE** `/draft-parts/{draftPartId}/hosts/{hostId}` — RemoveHost
- **PUT** `/draft-parts/{draftPartId}/hosts/{hostId}/primary` — SetPrimaryHost
- **PUT** `/draft-parts/{draftPartId}/hosts` — SetCoHosts (replace list, optional)

---

## Participants (DraftPart scoped)

- **POST** `/draft-parts/{draftPartId}/participants/drafters` — AddDrafterToDraftPart  
  Body: `{ "drafterId": "dr_..." }`
- **DELETE** `/draft-parts/{draftPartId}/participants/drafters/{drafterId}` — RemoveDrafterFromDraftPart

- **POST** `/draft-parts/{draftPartId}/participants/drafter-teams` — AddDrafterTeamToDraftPart  
  Body: `{ "drafterTeamId": "dt_..." }`
- **DELETE** `/draft-parts/{draftPartId}/participants/drafter-teams/{drafterTeamId}` — RemoveDrafterTeamFromDraftPart

- **PUT** `/draft-parts/{draftPartId}/participants/community` — SetCommunityParticipant (if explicit)

- **PUT** `/draft-parts/{draftPartId}/positions` — SetDraftPositions (if part-scoped)

---

## Picks (DraftPart scoped)

- **POST** `/draft-parts/{draftPartId}/picks` — AddPick  
  Body includes:
  - `movieId` (public id from Movies module)
  - `position` (final board position)
  - `playOrder` (sequence including vetoed picks)
  - `playedBy`: `{ "id": "...", "kind": "Drafter|DrafterTeam|Community" }`

- **DELETE** `/draft-parts/{draftPartId}/picks/{pickId}` — RemovePick (if allowed)
- **PUT** `/draft-parts/{draftPartId}/picks/{pickId}` — UpdatePick

### Vetoes & Overrides (actions on picks)
- **POST** `/draft-parts/{draftPartId}/picks/{pickId}/veto` — ApplyVeto
- **POST** `/draft-parts/{draftPartId}/picks/{pickId}/veto-override` — ApplyVetoOverride
- **POST** `/draft-parts/{draftPartId}/picks/{pickId}/commissioner-override` — ApplyCommissionerOverride
- **DELETE** `/draft-parts/{draftPartId}/picks/{pickId}/override` — UndoOverride (if allowed)

---

## Notes

- Avoid folder structures like `Post/CreateDraft`. HTTP verbs are transport-level details.
- Prefer organizing Vertical Slice folders by resource + use-case:
  - `DraftParts/Hosts/RemoveHost`
  - `DraftParts/Picks/ApplyVeto`
- Status changes should be one endpoint (`/status`) rather than 4 separate endpoints.
