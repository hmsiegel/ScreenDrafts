﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class BlessingType(string name, int value) : SmartEnum<BlessingType>(name, value)
{
  public static readonly BlessingType Veto = new("Veto", 0);
  public static readonly BlessingType VetoOverride = new("Veto Override", 1);
}
