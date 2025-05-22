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
      <h1>Hello {loaderData.name}</h1>
      <ul>
        <li>
          <Link to="/chats/1">Chat 1</Link>
        </li>
        <li>
          <Link to="/chats/2">Chat 2</Link>
        </li>
      </ul>
    </div>
  );
}