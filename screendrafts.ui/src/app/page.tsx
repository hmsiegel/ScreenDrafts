import Link from "next/link";
import { oswald, roboto } from "./ui/fonts";
import Image from "next/image";

export default function Home() {
  return (
    <div className="flex items-center justify-center min-h-screen ">
      <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center">
        <h1 className={`${oswald.className} text-6xl font-bold text-black mb-10`}>
          SCREEN DRAFTS
        </h1>

        <Image
          className="mb-5"
          src="/screen-drafts.jpg"
          alt="ScreenDrafts logo"
          height={400}
          width={400}
        />

        <div className="w-72 text-center ">
          <p className={`${roboto.className} font-bold`} >
            Where Experts And Enthusiasts Competitively Collaborate On The
            Creation Of Screen-centric Best-of Lists
          </p>
        </div>

        <div className="flex-row items-center justify-center">
          <Link href="/signin">
            <div className="btn-blue hover:bg-blue-400 hover:text-black transition ease-out duration-500">
              SIGN IN
            </div>
          </Link>

          <Link href="/register">
            <div className="btn-blue hover:bg-blue-400 hover:text-black transition ease-out duration-500">
              REGISTER
            </div>
          </Link>
        </div>
      </div>
    </div>
  )
}
