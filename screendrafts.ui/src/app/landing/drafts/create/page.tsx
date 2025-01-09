import CreateDraft from "@/app/ui/drafts/create-draft";
import Breadcrumbs from "@/app/ui/landing/breadcrumbs";
import { Metadata } from "next";

export const metadata: Metadata = {
   title: "Create Draft",
   description: "Create a new draft",
}

export default function Page() {
   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <Breadcrumbs
            breadcrumbs={[
               { label: "Home", href: "/" },
               { label: "Drafts", href: "/drafts" },
               { label: "Create Draft", href: "/drafts/create", active: true },
            ]}
         />
         <CreateDraft />
      </main>
   )
}