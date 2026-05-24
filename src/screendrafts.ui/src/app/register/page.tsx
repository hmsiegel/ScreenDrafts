"use client";

import { oswald } from "@/styles/fonts";
import RegisterForm from "@/features/register/components/register-form";
import Image from "next/image";
import { signIn } from "next-auth/react";

export default function RegisterPage() {
   return (
      <div className="sd-page-bg">
         <div className="flex flex-col items-center gap-6 px-4 py-12 w-full" style={{ maxWidth: 440 }}>
            {/* Brand */}
            <div className="flex flex-col items-center gap-3">
               <Image
                  src="/screen-drafts.jpg"
                  alt="ScreenDrafts"
                  width={96}
                  height={96}
                  className="rounded-xl shadow-lg"
               />
               <div className={`${oswald.className} text-2xl font-semibold tracking-widest text-[#fbf7ee] uppercase`}>
                  Screen Drafts
               </div>
            </div>

            {/* Card */}
            <div className="sd-register-card w-full">
               <h1 className="sd-register-title">Create Your Account</h1>
               <RegisterForm />
               <div className="sd-register-footer">
                  Already have an account?{" "}
                  <button onClick={() => signIn('keycloak', {callbackUrl: '/' })}>
                     Sign In
                  </button>
               </div>
            </div>
         </div>
      </div>
   );
}