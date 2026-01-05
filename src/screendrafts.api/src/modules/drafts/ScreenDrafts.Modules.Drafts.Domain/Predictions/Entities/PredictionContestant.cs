using ScreenDrafts.Modules.Drafts.Domain.Predictions.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class PredictionContestant : Entity<ContestantId>
{
  private PredictionContestant(
    Person person,
    ContestantId? id = null)
    : base(id ?? ContestantId.CreateUnique())
  {
    Person = person;
    PersonId = person.Id;
    DisplayName = person.DisplayName!;
  }

  private PredictionContestant()
  {
  }

  public Person Person { get; private set; } = default!;

  public PersonId? PersonId { get; private set; }     // link to People if available
  public string DisplayName { get; private set; } = "";
  public bool IsActive { get; private set; } = true;

  public static PredictionContestant Create(Person person, ContestantId? id = null)
  {
    ArgumentNullException.ThrowIfNull(person);
    return new PredictionContestant(person, id);
  }

  public void Deactivate()
  {
    IsActive = false;
  }
}
