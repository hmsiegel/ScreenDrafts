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
- **POST** `/series` — CreateSeries &#x2705;
- **GET** `/series` — SearchSeries
- **GET** `/series/{publicId}` — GetSeries &#x2705;
- **PATCH** `/series/{publicId}` — UpdateSeries &#x2705;
- **DELETE** `/series/{publicId}` — DeleteSeries
- **POST** `/series/{publicId}/restore` — RestoreSeries

### Campaigns
- **POST** `/campaigns` — CreateCampaign &#x2705;
- **GET** `/campaigns` — GetCampaigns &#x2705;
- **GET** `/campaigns/{publicId}` — GetCampaign &#x2705;
- **PATCH** `/campaigns/{publicId}` — UpdateCampaign &#x2705;
- **DELETE** `/campaigns/{publicId}` — DeleteCampaign &#x2705;
- **POST** `/campaigns/{publicId}/restore` — RestoreCampaign &#x2705;

### Categories
- **POST** `/categories` — CreateCategory &#x2705;
- **GET** `/categories` — GetCategories &#x2705;
- **GET** `/categories/{publicId}` — GetCategory &#x2705;
- **PATCH** `/categories/{publicId}` — UpdateCategory &#x2705;
- **DELETE** `/categories/{publicId}` — DeleteCategory &#x2705;
- **POST** `/categories/{publicId}/restore` — RestoreCategory &#x2705;

---

## People + Roles (Drafts domain)

> Users are owned by the Users module. Drafts owns People and role entities (Drafter/Host) that may link to a User.

### People
- **POST** `/people` — CreatePerson (optionally link to User publicId) &#x2705;
- **GET** `/people/{personId}` — GetPerson (optional) &#x2705;
- **GET** `/people` - ListPeople &#x2705;
- **GET** `/people` — SearchPeople (optional) &#x2705;

### Drafters (role)
- **POST** `/drafters` — CreateDrafter (promote Person → Drafter) &#x2705;
  Body: `{ "personId": "p_..." }`

### Hosts (role)
- **POST** `/hosts` — CreateHost (promote Person → Host) &#x2705;
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

- **POST** `/drafts` — CreateDraft &#x2705;
- **GET** `/drafts/{draftId}` — GetDraft
- **GET** `/drafts` — SearchDrafts
- **PUT** `/drafts/{draftId}` — UpdateDraftMetadata (includes name, description, soft delete) &#x2705;
- **PUT** `/drafts/{draftId}/category` — SetDraftCategory (single category) &#x2705;
- **PUT** `/drafts/{draftId}/categories` — SetDraftCategories (replace list) &#x2705;
- **PUT** `/drafts/{draftId}/episode` — SetEpisodeNumber &#x2705;
- **POST** `/drafts/{draftId}/campaign` — SetDraftCampaign &#x2705;
- **DELETE** `/drafts/{draftId}/campaign` — ClearDraftCampaign &#x2705;

---

## DraftParts (part-scoped gameplay)

### DraftParts core
- **POST** `/drafts/{draftId}/parts` — CreateDraftPart &#x2705;
- **GET** `/draft-parts/{draftPartId}` — GetDraftPart
- **GET** `/drafts/{draftId}/parts` — SearchDraftParts (optional/admin)
- **PUT** `/draft-parts/{draftPartId}` — UpdateDraftPartMetadata

### DraftPart status (single endpoint)
- **PUT** `/draft-parts/{draftPartId}/status` — SetDraftPartStatus &#x2705;
  Body: `{ "status": "InProgress" }`

---

## Releases (DraftPart scoped)

- **POST** `/draft-parts/{draftPartId}/releases` — AddReleaseDate &#x2705;
- **DELETE** `/draft-parts/{draftPartId}/releases/{releaseId}` — RemoveReleaseDate
- **POST** `/draft-parts/{draftPartId}/publish` — PublishRelease (if applicable)

---

## Hosts (DraftPart scoped)

- **POST** `/draft-parts/{draftPartId}/hosts` — AddHost &#x2705;
  Body: `{ "hostId": "h_...", "role": "Primary|CoHost" }`
- **DELETE** `/draft-parts/{draftPartId}/hosts/{hostId}` — RemoveHost
- **PUT** `/draft-parts/{draftPartId}/hosts/{hostId}/primary` — SetPrimaryHost
- **PUT** `/draft-parts/{draftPartId}/hosts` — SetCoHosts (replace list, optional)

---

## Participants (DraftPart scoped)
- **POST** `/draft-parts/{draftPartId}/participants/` - AddParticipant (generic, if we want to avoid separate endpoints)  
  Body: `{ "participantId": "...", "kind": "Drafter|DrafterTeam|Community" }`  &#x2705;
- **DELETE** `/draft-parts/{draftPartId}/participants/drafters/{drafterId}` — RemoveDrafterFromDraftPart
- **DELETE** `/draft-parts/{draftPartId}/participants/drafter-teams/{drafterTeamId}` — RemoveDrafterTeamFromDraftPart
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
