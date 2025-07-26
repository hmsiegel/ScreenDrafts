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

  [Column("twitter_handle")]
  public string? TwitterHandle { get; set; }

  [Column("bluesky_handle")]
  public string? BlueskyHandle { get; set; }

  [Column("instagram_handle")]
  public string? InstagramHandle { get; set; }

  [Column("letterboxd_handle")]
  public string? LetterboxdHandle { get; set; }

  [Column("profile_picture_url")]
  public string? ProfilePictureUrl { get; set; }
}
