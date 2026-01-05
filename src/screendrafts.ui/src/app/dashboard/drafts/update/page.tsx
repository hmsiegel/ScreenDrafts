import InitialDraftContextProvider from "@/features/drafts/contexts/initial-draft-context";
import UpdateDraft from "@/features/drafts/components/update-draft";
import Breadcrumbs from "@/components/ui/breadcrumbs";
import { Metadata } from "next";

export const metadata: Metadata = {
   title: "Update Draft",
   description: "Update a draft",
}

export default function Page() {
   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <InitialDraftContextProvider>
            <Breadcrumbs
               breadcrumbs={[
                  { label: "Home", href: "/" },
                  { label: "Main", href: "/main" },
                  { label: "Drafts", href: "/main/drafts" },
                  { label: "Update Draft", href: "/main/drafts/update", active: true },
               ]}
            />
            <UpdateDraft />
         </InitialDraftContextProvider>
      </main>
   )
}