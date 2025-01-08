import {
   ArrowRightCircleIcon,
   PencilIcon,
   PlayPauseIcon,
   TrashIcon } from "@heroicons/react/24/outline";
import Link from "next/link";

export function EditDraft({ id }: { id: string }) {
   return (
      <Link href={`/draft/${id}`}
         className="rounded-md border p-2 hover:bg-gray-100">
         <PencilIcon className="w-5" />
      </Link>
   )
}

export function StartDraft({ id }: { id: string }) {
   return (
      <Link href={`/draft/${id}`}
         className="rounded-md border p-2 hover:bg-gray-100">
         <ArrowRightCircleIcon className="w-5" />
      </Link>
   )
}

export function DeleteDraft({ id }: { id: string }) {
   return (
      <button className="rounded-md border p-2 hover:bg-gray-100">
         <TrashIcon className="w-5" />
      </button>
   )
}

export function PlayDraft({ id }: { id: string }) {
   return (
      <Link href={`/draft/${id}`}
         className="rounded-md border p-2 hover:bg-gray-100">
         <PlayPauseIcon className="w-5" />
      </Link>
   )
}