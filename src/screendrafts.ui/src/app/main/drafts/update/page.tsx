import InitialDraftContextProvider from "@/app/contexts/initial-draft-context";
import UpdateDraft from "@/app/ui/drafts/update-draft";
import Breadcrumbs from "@/app/ui/main/breadcrumbs";
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