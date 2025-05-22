import { Link } from "react-router";
import type { Route } from "./+types/index";

export async function loader({ params }: Route.LoaderArgs) {
  //let team = await fetchTeam(params.teamId);
  return { name: "Bob" };
}

export default function ChatsOverview({
  loaderData,
}: Route.ComponentProps) {
  return (
    <div>
      <h1>chats index</h1>
    </div>
  );
}