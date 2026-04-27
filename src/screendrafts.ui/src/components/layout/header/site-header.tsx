import Image from "next/image";
import Link from "next/link";
import { auth } from "@/auth";
import SignInButton from "./sign-in-button";
import AvatarDropdown from "./avatar-dropdown";

export default async function SiteHeader() {
  const session = await auth();
  const isAdmin = session?.roles?.includes('admin') ?? false;

  return (
    <header className="bg-white border-b-4 border-sd-red px-8 py-5 flex items-center justify-between">
      <div className="flex items-center gap-3.5">
        <Image
          src="/screen-drafts.jpg"
          alt="Screen Drafts logo"
          width={56}
          height={56}
          className="rounded-[10px]"
        />
        <div>
          <div className="font-oswald font-bold text-[32px] leading-none tracking-[0.02em] text-sd-ink">
            SCREEN DRAFTS
          </div>
          <div className="text-[11px] tracking-[0.22em] text-sd-blue mt-1">
            THE COMPETITIVELY-COLLABORATIVE BEST-OF-LIST PODCAST
          </div>
        </div>
      </div>

      <nav className="flex items-center gap-5 font-oswald font-medium text-sm tracking-[0.1em]">
        <Link href="/drafts" className="hover:text-sd-blue transition-colors">DRAFTS</Link>
        <Link href="/drafters" className="hover:text-sd-blue transition-colors">DRAFTERS</Link>
        <Link href="/films" className="hover:text-sd-blue transition-colors">FILMS</Link>
        <Link href="/wiki" className="hover:text-sd-blue transition-colors">WIKI</Link>
        {isAdmin && (
          <Link href="/admin" className="text-sd-red font-oswald font-semibold tracking-widest">
            ADMIN
          </Link>
        )}
        <Link href="/patreon" className="text-sd-red">★ PATREON</Link>

        {session ? (
          <AvatarDropdown name={session.user?.name} />
        ) : (
          <>
            <SignInButton />
            <Link
              href="/register"
              className="bg-sd-red text-white px-[18px] py-2.5 rounded font-oswald font-medium text-sm tracking-[0.14em] hover:bg-red-700 transition-colors"
            >
              REGISTER
            </Link>
          </>
        )}
      </nav>
    </header>
  );
}
