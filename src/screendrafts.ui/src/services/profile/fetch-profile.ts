import { auth } from "@/auth";
import { env } from "@/lib/env";

const apiBase = env.apiUrl;

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

// Shape returned by GET /users/profile (GetUserResponse)
export interface UserProfile {
  publicId: string;
  email: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  personPublicId?: string;
}

// Shape returned by GET /participants/{personPublicId} (GetParticipantResponse)
// Fields we care about for the profile page.
export interface PersonProfile {
  personPublicId: string;
  displayName: string;
  biography?: string;
  location?: string;
  profilePicturePath?: string;
  socialLinks?: { platform: string; url: string }[];
}

// Merged shape for the profile page — everything it needs in one object.
export interface MergedProfile extends UserProfile {
  // Resolved from PersonProfile
  displayName?: string;
  biography?: string;
  location?: string;
  profilePicturePath?: string;
  twitterHandle?: string;
  instagramHandle?: string;
  letterboxdHandle?: string;
  blueskyHandle?: string;
}

const SOCIAL_PLATFORM_MAP: Record<string, keyof Pick<MergedProfile, "twitterHandle" | "instagramHandle" | "letterboxdHandle" | "blueskyHandle">> = {
  twitter: "twitterHandle",
  instagram: "instagramHandle",
  letterboxd: "letterboxdHandle",
  bluesky: "blueskyHandle",
};

export async function fetchProfile(accessToken: string): Promise<MergedProfile | null> {
  const headers: HeadersInit = { Authorization: `Bearer ${accessToken}`};

  // Step 1: user record (email, name, personPublicId)
  let userProfile: UserProfile | null = null;
  try {
    const res = await fetch(`${apiBase}/users/profile`, {
      headers,
      next: { revalidate: 0 },
    });
    if (!res.ok) {
      console.error(`[fetchProfile] users/profile ${res.status}`);
      return null;
    }
    userProfile = await res.json() as UserProfile;
  } catch (err) {
    console.error("[fetchProfile] users/profile error:", err);
    return null;
  }

  // Step 2: person record (bio, location, socials, avatar) — optional
  if (!userProfile.personPublicId) {
    return { ...userProfile };
  }

  let personProfile: PersonProfile | null = null;
  try {
    const res = await fetch(
      `${apiBase}/participants/${userProfile.personPublicId}`,
      { headers, next: { revalidate: 0 } }
    );
    if (res.ok) {
      personProfile = await res.json() as PersonProfile;
    } else {
      console.warn(`[fetchProfile] participants/${userProfile.personPublicId} ${res.status}`);
    }
  } catch (err) {
    console.warn("[fetchProfile] participants fetch error:", err);
  }

  if (!personProfile) {
    return { ...userProfile };
  }

  // Flatten social links array into named handles
  const socials: Partial<MergedProfile> = {};
  for (const link of personProfile.socialLinks ?? []) {
    const key = SOCIAL_PLATFORM_MAP[link.platform.toLowerCase()];
    if (key) {
      // Store only the handle portion — strip leading @ and domain if present
      const raw = link.url;
      const handle = raw.startsWith("http")
        ? new URL(raw).pathname.replace(/^\//, "").split("/")[0]
        : raw.replace(/^@/, "");
      socials[key] = handle;
    }
  }

  return {
    ...userProfile,
    displayName: personProfile.displayName,
    biography: personProfile.biography,
    location: personProfile.location,
    profilePicturePath: personProfile.profilePicturePath,
    ...socials,
  };
}