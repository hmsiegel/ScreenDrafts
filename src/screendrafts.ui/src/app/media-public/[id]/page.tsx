// app/media-public/[id]/page.tsx
import { notFound, permanentRedirect } from "next/navigation";
import { fetchMediaDetailPublic } from "@/services/media/fetch-media-detail";
import { fetchMediaPublicIdByImdbId } from "@/services/media/fetch-media-public-id-by-imdb-id";
import MediaDetailView, { IMDB_ID_PATTERN } from "../../media/[id]/media-detail-view";

type Props = { params: Promise<{ id: string }> };

export const revalidate = 86400;

export async function generateMetadata({ params }: Props) {
  const { id } = await params;
  if (IMDB_ID_PATTERN.test(id)) return { title: "Media" };
  const media = await fetchMediaDetailPublic(id);
  return { title: media?.title ?? "Media" };
}

export default async function PublicMediaDetailPage({ params }: Props) {
  const { id } = await params;

  if (IMDB_ID_PATTERN.test(id)) {
    const publicId = await fetchMediaPublicIdByImdbId(id);
    if (publicId) permanentRedirect(`/media/${publicId}`);
    notFound();
  }

  const media = await fetchMediaDetailPublic(id);
  if (!media) notFound();
  return <MediaDetailView media={media} />;
}