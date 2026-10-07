// app/stats/titles/page.tsx
import { redirect } from "next/navigation";

// The Titles tab opens on the first level.
export default function TitlesIndexPage() {
  redirect("/stats/titles/marquee-of-fame");
}
