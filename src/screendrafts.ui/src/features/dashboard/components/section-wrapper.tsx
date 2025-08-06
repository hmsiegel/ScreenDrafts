import { ReactNode } from "react";
import { inter } from "../../../styles/fonts";
import Link from "next/link";
import { PlusIcon } from "@heroicons/react/24/outline";

interface Props {
   title: string;
   children: ReactNode;
}

export default function SectionWrapper({ title, children }: Props) {
   return (
      <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center my-4">
         {/* Title Section */}
         <div className="flex items-center justify-center border-b-2 border-slate-900">
            <h1 className={`${inter.className} text-2xl text-black uppercase`}>
               {title}
            </h1>
         </div>
         {/* Create Draft Button */}
         <div className="flex items-center justify-center">
            <Link href="/main/drafts/create" className="flex items-center justify-center btn-red hover:bg-red-400 hover:text-black transition ease-out duration-500">
               create draft
               <PlusIcon className="w-5 ml-2" />
            </Link>
         </div>
         {children}
      </div>
   );
}