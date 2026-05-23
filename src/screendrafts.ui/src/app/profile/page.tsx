import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { fetchProfile } from "@/services/profile/fetch-profile";
import SiteHeader from "@/components/layout/header/site-header";
import SiteFooter from "@/components/layout/footer/site-footer";
import ProfileForm from "./profile-form";
import { Metadata } from "next";
import { env } from "@/lib/env";

export const metadata: Metadata = { title: "Profile" };
export const dynamic = "force-dynamic";

function InitialsAvatar({ name }: { name: string }) {
  const parts = name.trim().split(/\s+/);
  const letters = parts.length >= 2
    ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
    : name.slice(0, 2).toUpperCase();
  return (
    <div className="w-[100px] h-[100px] rounded-full bg-sd-blue flex items-center justify-center">
      <span className="font-oswald font-bold text-[30px] text-white leading-none">{letters}</span>
    </div>
  );
}

export default async function ProfilePage() {
  const session = await auth();
  if (!session) redirect("/api/auth/signin?callbackUrl=/profile");

  const profile = await fetchProfile();
  const displayName = [profile?.firstName, profile?.lastName].filter(Boolean).join(' ')
    || session.user?.name
    || 'User';
  const email = profile?.email || session.user?.email || '';
  const apiBase = env.apiUrl ?? '';

  return (
    <div className="min-h-screen bg-light-blue">
      <SiteHeader activePath="/profile" />

      <div className="px-6 md:px-10 py-10 max-w-[1100px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">/ PROFILE</p>
        <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink mb-10">
          YOUR PROFILE
        </h1>

        <div className="flex flex-col md:flex-row gap-8">
          {/* Sidebar */}
          <aside className="md:w-[260px] shrink-0">
            <div className="bg-white border border-sd-ink/10 p-6 space-y-5">
              <div className="flex justify-center">
                {profile?.profilePicturePath ? (
                  <img
                    src={profile.profilePicturePath}
                    alt={displayName}
                    className="w-[100px] h-[100px] rounded-full object-cover border border-sd-ink/20"
                  />
                ) : (
                  <InitialsAvatar name={displayName} />
                )}
              </div>
              <div className="text-center space-y-1">
                <p className="font-oswald font-bold text-[20px] text-sd-ink leading-tight">
                  {displayName}
                </p>
                {email && (
                  <p className="font-mono text-[11px] text-sd-ink/50 break-all">{email}</p>
                )}
              </div>
              <div className="border-t border-sd-ink/10 pt-4 space-y-1">
                {profile?.twitterHandle && (
                  <p className="font-mono text-[11px] text-sd-ink/60">𝕏 @{profile.twitterHandle}</p>
                )}
                {profile?.instagramHandle && (
                  <p className="font-mono text-[11px] text-sd-ink/60">IG @{profile.instagramHandle}</p>
                )}
                {profile?.letterboxdHandle && (
                  <p className="font-mono text-[11px] text-sd-ink/60">LB {profile.letterboxdHandle}</p>
                )}
                {profile?.blueskyHandle && (
                  <p className="font-mono text-[11px] text-sd-ink/60">BSky {profile.blueskyHandle}</p>
                )}
              </div>
            </div>
          </aside>

          {/* Main content */}
          <main className="flex-1">
            <ProfileForm
              profile={profile}
              accessToken={session.accessToken}
              apiBase={apiBase}
            />
          </main>
        </div>
      </div>

      <SiteFooter />
    </div>
  );
}
