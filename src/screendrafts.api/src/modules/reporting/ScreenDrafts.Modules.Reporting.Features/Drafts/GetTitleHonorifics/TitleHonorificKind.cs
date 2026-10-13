namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>How a level decides which titles hold it.</summary>
internal enum TitleHonorificKind
{
  /// <summary>By the number of canonical episodes the title has been drafted on.</summary>
  Appearances,

  /// <summary>By finishing at No. 1 on two or more drafts.</summary>
  UnifiedNo1,

  /// <summary>By finishing at No. 1, No. 2, No. 3 and No. 4.</summary>
  TheCycle,
}
