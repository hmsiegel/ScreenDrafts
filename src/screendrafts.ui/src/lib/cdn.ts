// lib/cdn.ts
//
// Requires NEXT_PUBLIC_CDN_URL (inlined at build time — set it in Vercel and redeploy).
// Unset = relative URLs, i.e. the old behaviour.

const CDN_URL = (process.env.NEXT_PUBLIC_CDN_URL ?? "").replace(/\/+$/, "");

export type CdnFolder = "artifacts" | "drafters" | "drafts";

/**
 * Public URL for a stored file. `path` is the bare filename the API returns
 * (profilePicturePath, avatarPath, imagePath). Absolute URLs pass through.
 */
export function cdnUrl(folder: CdnFolder, path: string): string;
export function cdnUrl(folder: CdnFolder, path: string | null | undefined): string | undefined;
export function cdnUrl(folder: CdnFolder, path: string | null | undefined): string | undefined {
  if (!path) return undefined;
  if (/^https?:\/\//i.test(path)) return path;
  return `${CDN_URL}/${folder}/${path}`;
}

/** Badges, banners, logo: static files under artifacts/. */
export function artifactUrl(fileName: string): string {
  return `${CDN_URL}/artifacts/${fileName}`;
}

/**
 * The Screen Drafts logo, shown when a draft has no image. CDN copy first,
 * then the copy bundled in the UI's public folder in case the CDN is unreachable.
 */
export const DRAFT_IMAGE_FALLBACKS: readonly string[] = [
  artifactUrl("logo.jpg"),
  "/screen-drafts.jpg",
];

/**
 * Every URL a draft's image might live at, most specific first. Deduplicated.
 *
 * - New uploads: imagePath is `{publicId}-{8 hex}.{ext}`, so the first URL resolves.
 * - Pre-CDN images were copied to R2 as `{publicId}.jpg` or `{publicId}.webp`, with no
 *   hex suffix. Their imagePath may be missing or carry the other extension, so both
 *   legacy names follow as fallbacks.
 */
export function draftImageCandidates(
  publicId: string | null | undefined,
  imagePath: string | null | undefined
): string[] {
  const names = [imagePath, publicId ? `${publicId}.jpg` : null, publicId ? `${publicId}.webp` : null];
  const urls = names
    .filter((name): name is string => !!name)
    .map((name) => cdnUrl("drafts", name));
  return [...new Set(urls)];
}