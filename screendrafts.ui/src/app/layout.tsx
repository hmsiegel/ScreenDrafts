import type { Metadata } from "next";
import {roboto} from "@/app/ui/fonts";
import "@/app/ui/global.css";

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
        className={`${roboto.className} antialiased bg-background text-foreground`}
      >
        {children}
      </body>
    </html>
  );
}
