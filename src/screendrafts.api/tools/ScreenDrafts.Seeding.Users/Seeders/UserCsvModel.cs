namespace ScreenDrafts.Seeding.Users.Seeders;

internal sealed class UserCsvModel
{
  [Column("personId")]
  public Guid? PersonId { get; set; }

  [Column("last_name")]
  public string LastName { get; set; } = default!;

  [Column("first_name")]
  public string FirstName { get; set; } = default!;

  [Column("email_address")]
  public string EmailAddress { get; set; } = default!;
}
