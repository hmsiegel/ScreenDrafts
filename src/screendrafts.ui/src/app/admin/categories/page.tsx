import { auth } from "@/auth";
import { listAllCategories } from "@/services/admin/fetch-admin-categories";
import CategoryManager from "./category-manager";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Categories — ScreenDrafts Admin" };
export const dynamic = "force-dynamic";

export default async function CategoriesPage() {
  const session = await auth();
  const categories = await listAllCategories(true);

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">/ ADMIN / CATEGORIES</p>
        <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink mb-10">
          CATEGORIES
        </h1>
        <p className="font-serif italic text-[16px] text-sd-ink/70 max-w-2xl mb-10">
          Categories classify what a draft is about — the subject matter, format, or scope. A
          draft can carry multiple categories, and categories are searchable across the archive.
          Use them to make the collection discoverable.
        </p>
        <CategoryManager initialData={categories} accessToken={session?.accessToken} />
      </div>
    </div>
  );
}
