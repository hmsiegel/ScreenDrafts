namespace ScreenDrafts.Common.Domain;

public sealed class MediaType(string name, int value) : SmartEnum<MediaType>(name, value)
{
  public static readonly MediaType Movie = new(nameof(Movie), 0);
  public static readonly MediaType TvShow = new(nameof(TvShow), 1);
  public static readonly MediaType TvEpisode = new(nameof(TvEpisode), 2);
  public static readonly MediaType VideoGame = new(nameof(VideoGame), 3);
  public static readonly MediaType MusicVideo = new(nameof(MusicVideo), 4);
}
