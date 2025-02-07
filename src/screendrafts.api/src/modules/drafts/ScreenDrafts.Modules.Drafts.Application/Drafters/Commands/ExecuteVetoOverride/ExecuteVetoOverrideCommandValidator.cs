﻿namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVetoOverride;

internal sealed class ExecuteVetoOverrideCommandValidator : AbstractValidator<ExecuteVetoOverrideCommand>
{
  public ExecuteVetoOverrideCommandValidator()
  {
    RuleFor(x => x.DrafterId)
      .NotEmpty();
    RuleFor(x => x.VetoId)
      .NotEmpty();
  }
}
