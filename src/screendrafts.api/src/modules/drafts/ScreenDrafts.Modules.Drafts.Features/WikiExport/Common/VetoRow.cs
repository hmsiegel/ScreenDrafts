// Drafts module — Features/WikiExport/WikiExport.Common.cs
// Shared by ExportDrafts and ExportDrafters.

namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.Common;

internal sealed record VetoRow(
  Guid PickId,
  int Sequence,
  Guid IssuedByParticipantRowId,
  bool IsOverridden,
  Guid? OverriddenByParticipantRowId
);
