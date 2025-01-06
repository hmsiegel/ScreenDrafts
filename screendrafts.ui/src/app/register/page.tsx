import { oswald, inter } from "@/app/ui/fonts";
import RegisterForm from "@/app/ui/register-form";

export default function Home() {
   return (
      <div className="flex items-center justify-center min-h-screen ">
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center">
            <h1 className={`${oswald.className} text-6xl font-bold text-black mb-10`}>
               SCREEN DRAFTS
            </h1>

            <img
               className="mb-5"
               src="/screen-drafts.jpg"
               alt="ScreenDrafts logo"
               height={400}
               width={400}
            />
            <h2 className={`${inter.className} text-5xl text-black uppercase mb-8`}>
               register
            </h2>

            <RegisterForm />
         </div>
      </div>
   )
}
