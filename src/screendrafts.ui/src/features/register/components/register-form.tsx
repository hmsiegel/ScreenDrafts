// app/ui/register-form.tsx
'use client';

import { apiRequest } from "@/features/drafts/api/api";
import { useRouter } from "next/navigation";
import { ChangeEvent, ChangeEventHandler, useState } from "react";

export default function RegisterForm() {
   const router = useRouter();

   // local state to store the form data
   const [formData, setFormData] = useState({
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      confirmPassword: "",
   });
   const [isLoading, setIsLoading] = useState(false);
   const [error, setError] = useState<string | null>(null);

   // keep <input> values in sync with local state
   function handleChange(event: ChangeEvent<HTMLInputElement>) {
      setFormData({ ...formData, [event.target.name]: event.target.value });
   }

   // handle form submission
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
            headers: {
               "Content-Type": "application/json",
            },
            body: JSON.stringify({
               firstName: formData.firstName,
               lastName: formData.lastName,
               email: formData.email,
               password: formData.password,
            }),
         });

         // redirect to the login page
         router.push("/login");
      } catch (error) {
         setError((error as Error).message ?? 'An error occurred');
      } finally {
         setIsLoading(false);
      }
   }


   return (
      <form className="w-full max-w-sm" onSubmit={handleSubmit}>
         {/** First Name */}
         <InputBlock
            id="firstName"
            label="First Name"
            value={formData.firstName}
            onChange={handleChange}
         />
         {/** Last Name */}
         <InputBlock
            id="lastName"
            label="Last Name"
            value={formData.lastName}
            onChange={handleChange}
         />

         {/** Email */}
         <InputBlock
            id="email"
            type="email"
            label="Email"
            value={formData.email}
            onChange={handleChange}
         />

         {/** Password */}
         <InputBlock
            id="password"
            type="password"
            label="Password"
            value={formData.password}
            onChange={handleChange}
         />

         {/** Confirm Password */}
         <InputBlock
            id="confirmPassword"
            type="password"
            label="COnfirm Password"
            value={formData.confirmPassword}
            onChange={handleChange}
         />

         {/** Error message */}
         {error && (
            <div className="md:flex md:items-center mb-6">
               <div className="md:w-1/3"></div>
               <div className="md:w-2/3">
                  <p className="text-red-500 text-xs italic">{error}</p>
               </div>
            </div>
         )}

         {/** Submit button */}

         <div className="flex justify-center">
               <button
                  className="uppercase shadow bg-sd-red hover:bg-red-300 hover:text-black transition ease-out duration-200 focus:shadow-outline focus:outline-none text-white font-bold py-2 px-20 rounded my-3"
                  type="submit"
                  disabled={isLoading}>
                  {isLoading ? 'Creating...' : 'Create Account'}
               </button>
         </div>
      </form>
   );
}

// Input helper
function InputBlock(props: {
   id: string;
   label: string;
   value: string;
   onChange: ChangeEventHandler<HTMLInputElement>;
   type?: string;
}) {
   return (
      <div className="md:flex md:items-center mb-6">
         <label className="md:w-1/3 label" htmlFor={props.id}>
            {props.label}
         </label>
         <div className="md:w-2/3">
            <input
               {...props}
               name={props.id}
               className="input"
               autoComplete="off"
            />
         </div>
      </div>
   );
}
