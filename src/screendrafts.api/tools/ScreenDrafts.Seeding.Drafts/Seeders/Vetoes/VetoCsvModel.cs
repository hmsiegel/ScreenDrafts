namespace ScreenDrafts.Seeding.Drafts.Seeders.Vetoes;

internal sealed class VetoCsvModel
{
  public Guid IssuedByParticipantIdValue { get; set; }
  public int IssuedByParticipantKindValue {get; set;}
  public Guid DraftPartId { get; set; }
  public int Position { get; set; }
  public Guid MovieId { get; set; }
  public int PlayOrder { get; set; }
}
