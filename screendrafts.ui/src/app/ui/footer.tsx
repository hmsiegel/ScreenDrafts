export default function Footer() {
   return (
      <footer className="
        text-slate-200
        text-center
        text-sm
        fixed
        bottom-0
        inset-x-0
        bg-gradient-to-r
        from-sd-blue
        via-slate-500
        to-sd-blue">
         <div className="p-2">
            <p>
               &copy; ${new Date().getFullYear()} ScreenDrafts. All rights reserved.
            </p>
         </div>
      </footer>
   )
}