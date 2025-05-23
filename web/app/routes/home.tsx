import type { Route } from "./+types/home";
import { Link, redirect } from "react-router";

// export function meta({}: Route.MetaArgs) {
//   return [
//     { title: "New React Router App" },
//     { name: "description", content: "Welcome to React Router!" },
//   ];
// }

export async function loader() {
  return redirect("/chats");
}

export default function Home() {
  return <Link to="/chats">Chats</Link>;
}
