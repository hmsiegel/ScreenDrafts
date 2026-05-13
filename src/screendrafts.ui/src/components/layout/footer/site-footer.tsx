const SPOTIFY_URL = process.env.NEXT_PUBLIC_SPOTIFY_URL;
const APPLE_URL = process.env.NEXT_PUBLIC_APPLE_PODCASTS_URL;
const PATREON_URL = process.env.NEXT_PUBLIC_PATREON_URL;
const RSS_URL = process.env.NEXT_PUBLIC_RSS_URL;
const WIKI_URL = process.env.NEXT_PUBLIC_WIKI_URL;

const year = new Date().getFullYear();

function FooterLink({ href, children, external }: { href: string; children: React.ReactNode; external?: boolean }) {
  if (external) {
    return (
      <a
        href={href}
        target="_blank"
        rel="noopener noreferrer"
        className="text-light-blue hover:text-white transition-colors text-[13px]"
      >
        {children}
      </a>
    );
  }
  return (
    <a href={href} className="text-light-blue hover:text-white transition-colors text-[13px]">
      {children}
    </a>
  );
}

function ColHeader({ children }: { children: React.ReactNode }) {
  return (
    <div className="font-mono text-[10px] tracking-widest text-sd-red font-bold mb-3">
      {children}
    </div>
  );
}

export default function SiteFooter() {
  return (
    <footer className="bg-sd-ink text-light-blue">
      <div className="px-10 pt-12 pb-8 flex justify-between gap-12">
        {/* Left: branding + tagline */}
        <div className="max-w-sm">
          <div className="font-oswald font-bold text-[28px] text-white leading-none mb-4">
            SCREEN DRAFTS
          </div>
          <p className="font-serif italic text-[15px] leading-relaxed text-light-blue">
            &ldquo;Where experts and enthusiasts competitively collaborate on the creation of screen-centric
            best-of lists. Decisively not ending in a tie.&rdquo;
          </p>
        </div>

        {/* Right: two columns */}
        <div className="flex gap-16">
          <div>
            <ColHeader>LISTEN</ColHeader>
            <nav className="flex flex-col gap-2.5">
              {APPLE_URL && <FooterLink href={APPLE_URL} external>Apple Podcasts</FooterLink>}
              {SPOTIFY_URL && <FooterLink href={SPOTIFY_URL} external>Spotify</FooterLink>}
              {RSS_URL && <FooterLink href={RSS_URL} external>RSS</FooterLink>}
              {PATREON_URL && <FooterLink href={PATREON_URL} external>Patreon</FooterLink>}
            </nav>
          </div>
          <div>
            <ColHeader>EXPLORE</ColHeader>
            <nav className="flex flex-col gap-2.5">
              <FooterLink href="/drafts">Drafts Archive</FooterLink>
              <FooterLink href="/drafters">Drafters</FooterLink>
              <FooterLink href="/media">Films A–Z</FooterLink>
              {WIKI_URL && <FooterLink href={WIKI_URL} external>The Wiki</FooterLink>}
            </nav>
          </div>
        </div>
      </div>

      {/* Bottom rule */}
      <div className="border-t border-white/10 px-10 py-4 flex justify-between font-mono text-[10px] tracking-widest text-white/50">
        <span>© {year} SCREEN DRAFTS · A PODCAST IN GOOD STANDING</span>
        <span>BUILT FOR THE COMMISSIONERS</span>
      </div>
    </footer>
  );
}