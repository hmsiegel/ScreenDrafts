import Link from "next/link";
import Image from "next/image";
import logo from "../../../../public/screen-drafts.jpg"

export default function NavMenu() {
   return (
      <nav className="fixed top-0 flex justify-between items-center w-full px-4 py-2 bg-slate-900 text-white">
         <div className="flex items-center">

            <Link href="/" className="text-xl font-bold">
               <Image
                  src={logo}
                  className="rounded-md"
                  alt="Screen Drafts"
                  width={50}
                  height={50} />
            </Link>
         </div>
         <div className="flex items-center">
            <h1 className="text-5xl font-bold">Screen Drafts</h1>
         </div>
         <div className="flex items-center text-lg">
            <Link href="/drafts" className="mr-4">Drafts</Link>
            <Link href="/gms" className="mr-4">GMs</Link>
            <Link href="/stats" className="mr-4">Statistics</Link>
            <Link href="/profile" className="mr-4">Profile</Link>
         </div>
      </nav>
   )
}