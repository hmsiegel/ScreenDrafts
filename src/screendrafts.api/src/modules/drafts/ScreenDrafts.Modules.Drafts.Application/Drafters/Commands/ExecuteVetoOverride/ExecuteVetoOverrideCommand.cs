﻿namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVetoOverride;
public sealed record ExecuteVetoOverrideCommand(Guid? DrafterId, Guid? DrafterTeamId, Guid VetoId) : ICommand<Guid>;
