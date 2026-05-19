const MEDIA_TYPE_LABELS: Record<string, string> = {
  Movie: "Movie",
  TvShow: "TV Show",
  TvEpisode: "TV Episode",
  VideoGame: "Video Game",
};

export function mediaTypeLabel(mediaType: string): string {
  return MEDIA_TYPE_LABELS[mediaType] ?? mediaType;
}
