import LoginForm from "@/app/ui/login-form";
import { Metadata } from "next";


export const metadata: Metadata = {
   title: 'Login',
};

export default function LoginPage() {
   return (
      <main className="flex items-center justify-center md:h-screen">
         <div className="relative mx-auto flex w-full max-w-[400px] flex-col space-y-2.5 p-4 md:-mt-32">
            <div className="flex w-full items-end rounded-lg bg-sd-blue p-12">
                  <img
                     className="rounded-xl"
                     src="/screen-drafts.jpg"
                     alt="ScreenDrafts logo"
                     height={400}
                     width={400}
                  />
            </div>
            <LoginForm />
         </div>
      </main>
   );
}