// Drafts module — Features/WikiExport/WikiExport.Common.cs
// Shared by ExportDrafts and ExportDrafters.

namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.Common;

// ── Shared row shapes ─────────────────────────────────────────────────────
// Dapper maps these by constructor: SQL aliases must match the names AND the order.

internal sealed record PickRow(
  Guid PickId,
  Guid PartId,
  int Position,
  int PlayOrder,
  int SubDraftIndex,
  Guid ParticipantRowId,
  Guid MovieId,
  string Title,
  string? Year,
  bool WasCommissionerOverride
);
