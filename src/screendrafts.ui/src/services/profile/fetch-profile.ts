import { env } from "@/lib/env";

const apiBase = env.apiUrl;

export interface UserProfile {
  publicId: string;
  email: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  personPublicId?: string;
}

// Matches the actual GET /participants/{personPublicId} response shape.
interface SocialHandles {
  twitter?: string | null;
  instagram?: string | null;
  letterboxd?: string | null;
  bluesky?: string | null;
  profilePicturePath?: string | null;
}

interface PersonProfile {
  personPublicId: string;
  displayName: string;
  biography?: string;
  location?: string;
  socialHandles?: SocialHandles;
}

export interface MergedProfile extends UserProfile {
  displayName?: string;
  biography?: string;
  location?: string;
  profilePicturePath?: string;
  twitterHandle?: string;
  instagramHandle?: string;
  letterboxdHandle?: string;
  blueskyHandle?: string;
}

export async function fetchProfile(accessToken: string): Promise<MergedProfile | null> {
  const headers: HeadersInit = { Authorization: `Bearer ${accessToken}` };

  // Step 1: user record
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

  if (!userProfile.personPublicId) {
    return { ...userProfile };
  }

  // Step 2: person record
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

  const handles = personProfile.socialHandles;

  return {
    ...userProfile,
    displayName: personProfile.displayName,
    biography: personProfile.biography,
    location: personProfile.location,
    profilePicturePath: handles?.profilePicturePath
      ? `${apiBase}/drafters/${handles.profilePicturePath}`
      : undefined,
    twitterHandle:   handles?.twitter    ?? undefined,
    instagramHandle: handles?.instagram  ?? undefined,
    letterboxdHandle: handles?.letterboxd ?? undefined,
    blueskyHandle:   handles?.bluesky    ?? undefined,
  };
}