// app/media/[id]/page.tsx
import { notFound, permanentRedirect } from "next/navigation";
import { fetchMediaDetail } from "@/services/media/fetch-media-detail";
import { fetchMediaPublicIdByImdbId } from "@/services/media/fetch-media-public-id-by-imdb-id";
import type { MediaResponse } from "@/lib/dto";
import MediaDetailView, { IMDB_ID_PATTERN } from "./media-detail-view";

type Props = { params: Promise<{ id: string }> };

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: Props) {
  const { id } = await params;
  if (IMDB_ID_PATTERN.test(id)) return { title: "Media" };
  try {
    const media = await fetchMediaDetail(id);
    return { title: media.title ?? "Media" };
  } catch {
    return { title: "Media" };
  }
}

export default async function MediaDetailPage({ params }: Props) {
  const { id } = await params;

  if (IMDB_ID_PATTERN.test(id)) {
    const publicId = await fetchMediaPublicIdByImdbId(id);
    if (publicId) permanentRedirect(`/media/${publicId}`);
    notFound();
  }

  let media: MediaResponse;
  try {
    media = await fetchMediaDetail(id);
  } catch {
    notFound();
  }

  return <MediaDetailView media={media} />;
}