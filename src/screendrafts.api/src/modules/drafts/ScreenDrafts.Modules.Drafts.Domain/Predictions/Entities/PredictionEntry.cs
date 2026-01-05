namespace ScreenDrafts.Modules.Drafts.Domain.Predictions.Entities;

public sealed class PredictionEntry : Entity<PredictionEntryId>
{
  private PredictionEntry(
    DraftPredictionSet predictionSet,
    Movie movie,
    int? orderIndex = null,
    string? notes = null,
    PredictionEntryId? id = null)
    : base(id ?? PredictionEntryId.CreateUnique())
  {
    PredictionSet = predictionSet;
    SetId = predictionSet.Id;

    Movie = movie;
    MovieId = movie.Id;

    OrderIndex = orderIndex;
    Notes = notes;
  }

  private PredictionEntry()
  {
  }

  public DraftPredictionSet PredictionSet { get; private set; } = default!;
  public DraftPredictionSetId SetId { get; private set; } = default!;

  public Movie Movie { get; private set; } = default!;
  public Guid MovieId { get; private set; } = Guid.Empty;

  public int? OrderIndex { get; private set; }          // null when unordered
  public string? Notes { get; private set; }

  public static PredictionEntry Create(
    DraftPredictionSet predictionSet,
    Movie movie,
    int? orderIndex = null,
    string? notes = null)
  {
    ArgumentNullException.ThrowIfNull(predictionSet);
    ArgumentNullException.ThrowIfNull(movie);

    return new PredictionEntry(
      predictionSet: predictionSet,
      movie: movie,
      orderIndex: orderIndex,
      notes: notes);
  }
}
