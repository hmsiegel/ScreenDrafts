'use client'

import {
   PencilIcon,
   PlayPauseIcon,
   TrashIcon
} from "@heroicons/react/24/outline";
import { Tooltip } from "@/components/ui/tooltip";
import Link from "next/link";
import clsx from "clsx";
import { useTransition } from "react";

type MutateFn = () => Promise<void> | void;

interface ButtonProps {
   id: string;
   onClick?: MutateFn;
}

/* -------------------------------------------------------------
 * Generic shell used for all buttons
 * ------------------------------------------------------------- */

function ActionButton({
   tooltip,
   href,
   onClick,
   children
}: {
   tooltip: string;
   href?: string;
   onClick?: MutateFn;
   children: React.ReactNode;
}) {
   const [isPending, startTransition] = useTransition();

   const handleClick = () => {
      if (!onClick) return;
      startTransition(async () => {
         try {
            await onClick();
         } catch (error) {
            console.error("Error executing action:", error);
         }
      });
   };

   const className = clsx(
      "rounded-md border p-2 hover:bg-gray-100 transition-colors",
      isPending && "opacity-50 pointer-events-none"
   );

   return (
      <Tooltip content={tooltip}>
         {href ? (
            <Link href={href} className={className}>
               {children}
            </Link>
         ) : (
            <button onClick={handleClick} className={className} disabled={isPending}>
               {children}
            </button>
         )}
      </Tooltip>
   );
}

export const EditDraft = ({ id }: ButtonProps) => (
   <ActionButton tooltip="Edit Draft" href={`/draft/${id}`}>
      <PencilIcon className="w-5" />
   </ActionButton>
)

export const StartDraft = ({ id, onClick }: ButtonProps) => (
   <ActionButton tooltip="Start Draft" onClick={onClick}>
      <PlayPauseIcon className="w-5" />
   </ActionButton>
)

export const PlayDraft = ({ id, onClick }: ButtonProps) => (
   <ActionButton tooltip="Play Draft" onClick={onClick}>
      <PlayPauseIcon className="w-5" />
   </ActionButton>
)

export const DeleteDraft = ({ id, onClick }: ButtonProps) => (
   <ActionButton tooltip="Delete Draft" onClick={onClick}>
      <TrashIcon className="w-5" />
   </ActionButton>
)