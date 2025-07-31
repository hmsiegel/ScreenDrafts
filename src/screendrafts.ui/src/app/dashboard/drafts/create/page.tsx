import InitialDraftContextProvider from "@/features/drafts/contexts/initial-draft-context";
import CreateDraft from "@/features/drafts/components/create-draft";
import Breadcrumbs from "@/components/ui/breadcrumbs";
import { Metadata } from "next";

export const metadata: Metadata = {
   title: "Create Draft",
   description: "Create a new draft",
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
                  { label: "Create Draft", href: "/main/drafts/create", active: true },
               ]}
            />
            <CreateDraft />
         </InitialDraftContextProvider>
      </main>
   )
}