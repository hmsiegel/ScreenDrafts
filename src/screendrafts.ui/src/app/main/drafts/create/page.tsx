import InitialDraftContextProvider from "@/app/contexts/initial-draft-context";
import CreateDraft from "@/app/ui/drafts/create-draft";
import Breadcrumbs from "@/app/ui/main/breadcrumbs";
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