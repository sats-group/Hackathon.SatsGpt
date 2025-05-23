import type { Route } from "./+types/summarize";
import { summarizeChat } from "~/lib/chat.server";

export async function action({ params }: Route.ActionArgs) {
  const summary = await summarizeChat({ id: params.chatId });
  return { name: summary };
}