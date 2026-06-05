import type { Metadata } from "next";
import { roboto } from "@/styles/fonts";
import "@/styles/global.css";
import Providers from "./providers";
import SiteHeader from "@/components/layout/header/site-header";
import SiteFooter from "@/components/layout/footer/site-footer";

export const metadata: Metadata = {
  title: {
    template: "%s | ScreenDrafts",
    default: "ScreenDrafts",
  },
  description: 'Where experts and enthusiasts competitively collaborate in the creation of screen-centric "Best Of" lists',
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        className={`${roboto.className} antialiased bg-gradient-to-t from-background via-slate-400 to-sd-blue text-foreground`}
      >
        <Providers>
          <SiteHeader />
          {children}
          <SiteFooter />
        </Providers>
      </body>
    </html>
  )
}
