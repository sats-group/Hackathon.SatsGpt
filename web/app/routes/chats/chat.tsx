import type { Route } from "./+types/chat";

export async function loader({ params }: Route.LoaderArgs) {
  //let team = await fetchTeam(params.teamId);
  return { chatId: params.chatId };
}

export default function Chat({
  loaderData,
}: Route.ComponentProps) {
  return <h1>Chat #{loaderData.chatId}</h1>;
}