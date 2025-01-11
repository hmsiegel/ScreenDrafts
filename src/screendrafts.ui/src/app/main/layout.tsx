import NavMenu from "@/app/ui/main/navmenu";
import Heading from "@/app/ui/main/heading";

export default function Layout({ children }: { children: React.ReactNode }) {
   return (
      <div className="flex flex-col items-center justify-center">
         <Heading />
         <NavMenu />
         <div>
            {children}
         </div>
      </div>
   )
}