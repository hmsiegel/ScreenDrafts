import NavMenu from "@/app/ui/landing/navmenu";

export default function Layout({ children }: { children: React.ReactNode }) {
   return (
      <div>
         <nav>
            <NavMenu />
         </nav>
         <div>
            {children}
         </div>
      </div>
   )
}