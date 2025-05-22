import { Button } from "~/components/ui/button";
import type { Route } from "./+types/chat";
import { cn } from "~/lib/utils";

export async function loader({ params }: Route.LoaderArgs) {
  //let chat = await fetchChat(params.chatId);
  return { chatId: params.chatId };
}

interface Message {
  id: string;
  content: string;
  role: "user" | "assistant";
}

export default function Chat({
  loaderData,
}: Route.ComponentProps) {
  // This would typically come from your state management
  const messages: Message[] = [
    {
      id: "1",
      content: "Hello! How can I help you today?",
      role: "assistant",
    },
    {
      id: "2",
      content: "I have a question about...",
      role: "user",
    },
  ];

  return (
    <div className="flex h-[calc(100vh-4rem)] flex-col">
      <div className="flex-1 overflow-y-auto p-4">
        <div className="mx-auto max-w-3xl space-y-4">
          {messages.map((message) => (
            <div
              key={message.id}
              className={cn(
                "flex w-full",
                message.role === "user" ? "justify-end" : "justify-start"
              )}
            >
              <div
                className={cn(
                  "max-w-[80%] rounded-lg px-4 py-2",
                  message.role === "user"
                    ? "bg-muted"
                    : ""
                )}
              >
                <p className="text-sm">{message.content}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
      <div className="p-4">
        <div className="mx-auto max-w-3xl">
          <div className="flex gap-2 items-start">
            <textarea
              rows={3}
              placeholder="Type your message..."
              className="flex-1 resize-none rounded-2xl border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            />
            <Button
              variant="default"
              size="lg"
            >
              Send
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}