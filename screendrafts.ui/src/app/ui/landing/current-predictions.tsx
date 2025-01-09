import { inter, roboto } from "@/app/ui/fonts";

export default function CurrentPredictions() {
   return (
      <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center my-4">
         <h1 className={`${inter.className} text-2xl text-black mb-5 uppercase border-b-2 border-slate-900 pb-2`}>
            Current Prediction Points
         </h1>
         <table className="table-fixed w-max">
            <tbody className={`${roboto.className} text-center`}>
               <tr>
                  <td className="text-lg px-4 py-2 uppercase font-bold">Clay</td>
                  <td className="text-lg px-4 py-2 uppercase font-bold">91</td>
               </tr>
               <tr>
                  <td className="text-lg px-4 py-2 uppercase font-bold">Ryan</td>
                  <td className="text-lg px-4 py-2 uppercase font-bold">78</td>
               </tr>
            </tbody>
         </table>
      </div>
   )
}