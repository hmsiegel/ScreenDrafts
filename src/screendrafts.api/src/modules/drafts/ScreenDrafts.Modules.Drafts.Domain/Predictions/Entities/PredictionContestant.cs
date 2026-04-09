namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class PredictionContestant : Entity<ContestantId>
{
  private PredictionContestant(
    string publicId,
    Person person,
    ContestantId? id = null)
    : base(id ?? ContestantId.CreateUnique())
  {
    PublicId = publicId;
    Person = person;
    PersonId = person.Id;
    DisplayName = person.DisplayName!;
  }

  private PredictionContestant()
  {
  }

  public string PublicId { get; private set; } = default!;
  public Person Person { get; private set; } = default!;
  public PersonId? PersonId { get; private set; }     // link to People if available
  public string DisplayName { get; private set; } = "";
  public bool IsActive { get; private set; } = true;

  public static PredictionContestant Create(Person person, string publicId, ContestantId? id = null)
  {
    ArgumentNullException.ThrowIfNull(person);
    return new PredictionContestant(
      person: person,
      publicId: publicId,
      id: id);
  }

  public void Deactivate()
  {
    IsActive = false;
  }
}
