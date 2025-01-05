import { oswald, roboto } from "@/app/ui/fonts";

export default function Home() {
  return (
    <div className="flex items-center justify-center min-h-screen ">
      <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center">
        <h1 className={`${oswald.className} text-6xl font-bold text-black mb-10`}>
          SCREEN DRAFTS
        </h1>

        <img
          className="mb-10"
          src="/screen-drafts.jpg"
          alt="ScreenDrafts logo"
          height={400}
          width={400}
        />

        <div className="flex-row items-center justify-center">
          <div className="btn hover:bg-blue-400 hover:text-black transition ease-out duration-500">
            SIGN IN
          </div>

          <div className="btn hover:bg-blue-400 hover:text-black transition ease-out duration-500">
            REGISTER
          </div>
        </div>
      </div>
    </div>
  )
}
