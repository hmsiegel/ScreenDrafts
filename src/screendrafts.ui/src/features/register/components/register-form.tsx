'use client';

import { apiRequest } from "@/features/drafts/api/api";
import { useRouter } from "next/navigation";
import { ChangeEvent, ChangeEventHandler, useState } from "react";

export default function RegisterForm() {
   const router = useRouter();

   const [formData, setFormData] = useState({
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      confirmPassword: "",
   });
   const [isLoading, setIsLoading] = useState(false);
   const [error, setError] = useState<string | null>(null);

   function handleChange(event: ChangeEvent<HTMLInputElement>) {
      setFormData({ ...formData, [event.target.name]: event.target.value });
   }

   async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
      event.preventDefault();

      if (formData.password !== formData.confirmPassword) {
         setError("Passwords do not match");
         return;
      }

      setIsLoading(true);
      setError(null);

      try {
         await apiRequest("/users/register", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
               firstName: formData.firstName,
               lastName: formData.lastName,
               email: formData.email,
               password: formData.password,
            }),
         });
         router.push("/login");
      } catch (err) {
         setError((err as Error).message ?? "An error occurred");
      } finally {
         setIsLoading(false);
      }
   }

   return (
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
         <InputBlock id="firstName" label="First Name" value={formData.firstName} onChange={handleChange} />
         <InputBlock id="lastName" label="Last Name" value={formData.lastName} onChange={handleChange} />
         <InputBlock id="email" label="Email" value={formData.email} onChange={handleChange} type="email" />
         <InputBlock id="password" label="Password" value={formData.password} onChange={handleChange} type="password" />
         <InputBlock id="confirmPassword" label="Confirm Password" value={formData.confirmPassword} onChange={handleChange} type="password" />

         {error && (
            <div className="sd-alert sd-alert-error">
               {error}
            </div>
         )}

         <button
            type="submit"
            disabled={isLoading}
            className="sd-register-btn mt-1"
         >
            {isLoading ? "Creating Account..." : "Create Account"}
         </button>
      </form>
   );
}

function InputBlock(props: {
   id: string;
   label: string;
   value: string;
   onChange: ChangeEventHandler<HTMLInputElement>;
   type?: string;
}) {
   return (
      <div className="flex flex-col gap-1">
         <label htmlFor={props.id} className="sd-register-label">
            {props.label}
         </label>
         <input
            id={props.id}
            name={props.id}
            type={props.type ?? "text"}
            value={props.value}
            onChange={props.onChange}
            autoComplete="off"
            className="sd-register-input"
         />
      </div>
   );
}