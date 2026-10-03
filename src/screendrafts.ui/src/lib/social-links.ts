// lib/social-links.ts
//
// People store social *handles* (drafts.people.twitter_handle, etc.), not URLs.
// The API passes them through unchanged, so anything that renders a link has to
// build the URL — that's socialUrl(). normalizeSocialHandle() cleans what a
// user types into the profile form, since people paste "@name" or a whole
// profile URL as often as a bare handle.

export type SocialPlatform = 'twitter' | 'instagram' | 'letterboxd' | 'bluesky';

const PROFILE_URL: Record<SocialPlatform, (handle: string) => string> = {
  twitter: (h) => `https://x.com/${h}`,
  instagram: (h) => `https://www.instagram.com/${h}/`,
  letterboxd: (h) => `https://letterboxd.com/${h}/`,
  // Bluesky handles are domains (name.bsky.social, or a custom domain).
  bluesky: (h) => `https://bsky.app/profile/${h}`,
};

// Path prefix in front of the handle on each platform's profile URL.
const URL_PREFIX: Record<SocialPlatform, RegExp> = {
  twitter: /^(?:https?:\/\/)?(?:www\.|mobile\.)?(?:x|twitter)\.com\//i,
  instagram: /^(?:https?:\/\/)?(?:www\.)?instagram\.com\//i,
  letterboxd: /^(?:https?:\/\/)?(?:www\.)?(?:letterboxd\.com|boxd\.it)\//i,
  bluesky: /^(?:https?:\/\/)?(?:www\.)?bsky\.app\/profile\//i,
};

/**
 * Reduces whatever was typed to a bare handle: "@name", "x.com/name",
 * "https://www.instagram.com/name/?hl=en" → "name". Empty input → null.
 */
export function normalizeSocialHandle(platform: SocialPlatform, raw: string | null | undefined): string | null {
  let value = (raw ?? '').trim();
  if (!value) return null;

  value = value.replace(URL_PREFIX[platform], '');
  value = value.split(/[?#]/)[0];       // query string / fragment
  value = value.split('/')[0];          // anything after the handle segment
  value = value.replace(/^@+/, '');     // leading @

  return value || null;
}

/**
 * Profile URL for a stored handle. A value that is already a full URL (entered
 * before handles were normalized) is passed through as-is.
 */
export function socialUrl(platform: SocialPlatform, handle: string | null | undefined): string | null {
  const value = (handle ?? '').trim();
  if (!value) return null;
  if (/^https?:\/\//i.test(value)) return value;
  const clean = normalizeSocialHandle(platform, value);
  return clean ? PROFILE_URL[platform](encodeURIComponent(clean)) : null;
}