namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

/// <summary>
/// Records that a specific drafter should get individual credit for a pick actually
/// played by a DrafterTeam. Snapshotted once, at the moment the pick is created, from
/// whichever drafters happened to be on the team at that instant — deliberately NOT a
/// live join through current team membership, since membership drifts over time (e.g.
/// drafters graduating into "Legends" teams as they hit appearance milestones). A live
/// join would let a drafter who joined the team later retroactively claim a pick from
/// before they were a member.
///
/// Team-played picks intentionally do NOT also credit the team itself in per-drafter
/// stats — this replaces team-level credit, it doesn't sit alongside it. See
/// GetDrafterProfileQueryHandler's filmsDraftedSql/pickHistorySql for the read side.
/// </summary>
public sealed class TeamPickCredit : Entity<TeamPickCreditId>
{
  private TeamPickCredit(Pick pick, Guid drafterIdValue, TeamPickCreditId? id = null)
    : base(id ?? TeamPickCreditId.CreateUnique())
  {
    TargetPick = pick;
    TargetPickId = pick.Id;
    DrafterIdValue = drafterIdValue;
  }

  private TeamPickCredit() { }

  public Pick TargetPick { get; private set; } = default!;
  public PickId TargetPickId { get; private set; } = default!;

  /// <summary>
  /// The credited drafter's internal id (drafts.drafters.id) — not a Participant, since
  /// a team-pick credit is always to an individual Drafter, never another Team or Community.
  /// </summary>
  public Guid DrafterIdValue { get; private set; }

  internal static TeamPickCredit Create(
    Pick pick,
    Guid drafterIdValue,
    TeamPickCreditId? id = null
  ) => new(pick, drafterIdValue, id);
}
