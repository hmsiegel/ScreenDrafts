export interface MediaListItem {
  publicId: string;
  title: string;
  year: number | null;
  mediaType: string;
  posterPath: string | null;
  imdbId: string | null;
  tmdbId: number | null;
  draftCount: number;
}

export interface DraftAppearance {
  draftPublicId: string;
  draftTitle: string;
  episodeNumber: number | null;
  pickedBy: string;
  position: number | null;
  wasVetoed: boolean;
}

export interface MediaDetail extends MediaListItem {
  backdropPath: string | null;
  plot: string | null;
  genres: string[];
  directors: string[];
  actors: string[];
  draftAppearances: DraftAppearance[];
}

export interface MediaPagedResult {
  items: MediaListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}
