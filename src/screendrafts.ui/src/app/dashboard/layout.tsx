import NavMenu from "@/components/layout/menu/navmenu";
import Heading from "@/components/layout/header/heading";

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