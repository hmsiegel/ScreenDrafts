import { oswald, inter } from "@/app/ui/fonts";
import Link from "next/link";

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
        <h2 className={`${inter.className} text-5xl text-black uppercase`}>
          sign in
        </h2>

        <div className="flex-row items-center justify-center">

          <Link href="/signin/email">
            <div className="btn-red hover:bg-red-300 hover:text-black transition ease-out duration-500">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="size-6 inline-block mr-2">
                <path d="M1.5 8.67v8.58a3 3 0 0 0 3 3h15a3 3 0 0 0 3-3V8.67l-8.928 5.493a3 3 0 0 1-3.144 0L1.5 8.67Z" />
                <path d="M22.5 6.908V6.75a3 3 0 0 0-3-3h-15a3 3 0 0 0-3 3v.158l9.714 5.978a1.5 1.5 0 0 0 1.572 0L22.5 6.908Z" />
              </svg>
              <span>email</span>
            </div>
          </Link>

          <div className="btn-red hover:bg-red-300 hover:text-black transition ease-out duration-500">
            <svg xmlns="http://www.w3.org/2000/svg" className="size-6 inline-block mr-2" fill="currentColor" viewBox="0 0 24 24">
              <path d="M12.545,12.151L12.545,12.151c0,1.054,0.855,1.909,1.909,1.909h3.536c-0.607,1.972-2.101,3.467-4.26,3.866 c-3.431,0.635-6.862-1.865-7.19-5.339c-0.34-3.595,2.479-6.62,6.005-6.62c1.002,0,1.946,0.246,2.777,0.679 c0.757,0.395,1.683,0.236,2.286-0.368l0,0c0.954-0.954,0.701-2.563-0.498-3.179c-1.678-0.862-3.631-1.264-5.692-1.038 c-4.583,0.502-8.31,4.226-8.812,8.809C1.945,16.9,6.649,22,12.545,22c6.368,0,8.972-4.515,9.499-8.398 c0.242-1.78-1.182-3.352-2.978-3.354l-4.61-0.006C13.401,10.24,12.545,11.095,12.545,12.151z"></path>
            </svg>
            google
          </div>

          <div className="btn-red hover:bg-red-300 hover:text-black transition ease-out duration-500">
            <svg xmlns="http://www.w3.org/2000/svg" className="size-6 inline-block mr-2" fill="currentColor" viewBox="0 0 24 24">
              <path d="M17.525,9H14V7c0-1.032,0.084-1.682,1.563-1.682h1.868v-3.18C16.522,2.044,15.608,1.998,14.693,2 C11.98,2,10,3.657,10,6.699V9H7v4l3-0.001V22h4v-9.003l3.066-0.001L17.525,9z"></path>
            </svg>
            facebook
          </div>
        </div>
      </div>
    </div>
  )
}
