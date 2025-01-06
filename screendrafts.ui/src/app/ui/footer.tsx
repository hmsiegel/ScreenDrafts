export default function Footer() {
   return (
      <footer className="
        text-slate-950
        text-center
        text-sm
        fixed
        bottom-0
        inset-x-0
        bg-sd-blue">
         <div className="p-2">
            <p>
               &copy; ${new Date().getFullYear()} ScreenDrafts. All rights reserved.
            </p>
         </div>
      </footer>
   )
}