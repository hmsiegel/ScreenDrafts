import {
   ArrowRightCircleIcon,
   PencilIcon,
   PlayPauseIcon,
   TrashIcon
} from "@heroicons/react/24/outline";
import { Tooltip } from "@/app/ui/tooltip";
import Link from "next/link";

export function EditDraft({ id }: { id: string }) {
   return (
      <Tooltip content="Edit Draft" >
         <Link href={`/draft/${id}`}
            className="rounded-md border p-2 hover:bg-gray-100">
            <PencilIcon className="w-5" />
         </Link>
      </Tooltip>
   )
}

export function StartDraft({ id }: { id: string }) {
   return (
      <Tooltip content="Start Draft" >
         <Link href={`/draft/${id}`}
            className="rounded-md border p-2 hover:bg-gray-100">
            <ArrowRightCircleIcon className="w-5" />
         </Link>
      </Tooltip>
   )
}

export function DeleteDraft({ id }: { id: string }) {
   return (
      <Tooltip content="Delete Draft" >
         <button className="rounded-md border p-2 hover:bg-gray-100">
            <TrashIcon className="w-5" />
         </button>
      </Tooltip>
   )
}

export function PlayDraft({ id }: { id: string }) {
   return (
      <Tooltip content="Play Draft" >
         <Link href={`/draft/${id}`}
            className="rounded-md border p-2 hover:bg-gray-100">
            <PlayPauseIcon className="w-5" />
         </Link>
      </Tooltip>
   )
}