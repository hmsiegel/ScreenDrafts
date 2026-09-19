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