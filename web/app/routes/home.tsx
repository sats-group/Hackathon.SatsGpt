import type { Route } from "./+types/home";
import { Link, redirect } from "react-router";

// biome-ignore lint/correctness/noEmptyPattern: it is what it is
export function meta({}: Route.MetaArgs) {
  return [
    { title: "SATS GPT" },
    { name: "description", content: "Your SATS Knowledge Assistant" },
  ];
}

export async function loader() {
  return redirect("/chats");
}

export default function Home() {
  return <Link to="/chats">Chats</Link>;
}
