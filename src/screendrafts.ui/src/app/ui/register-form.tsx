
export default function LoginForm() {
   /*   const [errorMessage, formAction, isPending] = useActionState(
       authenticate,
       undefined,
     ); */
   return (
      <form className="w-full max-w-sm">
         <div className="md:flex md:items-center mb-6">
            <div className="md:w-1/3">
               <label className="label" htmlFor="inline-first-name">
                  First Name
               </label>
            </div>
            <div className="md:w-2/3">
               <input className="input" id="inline-first-name" type="text" />
            </div>
         </div>
         <div className="md:flex md:items-center mb-6">
            <div className="md:w-1/3">
               <label className="label" htmlFor="inline-last-name">
                  Last Name
               </label>
            </div>
            <div className="md:w-2/3">
               <input className="input" id="inline-last-name" type="text" />
            </div>
         </div>
         <div className="md:flex md:items-center mb-6">
            <div className="md:w-1/3">
               <label className="label" htmlFor="inline-email">
                  email
               </label>
            </div>
            <div className="md:w-2/3">
               <input className="input" id="inline-email" type="text" />
            </div>
         </div>
         <div className="md:flex md:items-center mb-6">
            <div className="md:w-1/3">
               <label className="label" htmlFor="inline-password">
                  Password
               </label>
            </div>
            <div className="md:w-2/3">
               <input className="input" id="inline-password" type="password" />
            </div>
         </div>
         <div className="md:flex md:items-center mb-6">
            <div className="md:w-1/3">
               <label className="label" htmlFor="inline-confirm-password">
                  Confirm Password
               </label>
            </div>
            <div className="md:w-2/3">
               <input className="input" id="inline-confirm-password" type="password" />
            </div>
         </div>
         <div className="md:flex md:items-center">
            <div className="md:w-1/3"></div>
            <div className="md:w-2/3">
               <button className="uppercase shadow bg-sd-red hover:bg-red-300 hover:text-black transition ease-out duration-500 focus:shadow-outline focus:outline-none text-white font-bold py-2 px-20 rounded my-3" type="button">
                  submit
               </button>
            </div>
         </div>
      </form>
   );
}
