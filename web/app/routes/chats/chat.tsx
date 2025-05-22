import { Button } from "~/components/ui/button";
import type { Route } from "./+types/chat";
import { cn } from "~/lib/utils";
import { useState, useEffect } from "react";
import { useFetcher } from "react-router";
import { Loader2 } from "lucide-react";
import { updateChat } from "~/lib/chat";
import invariant from "tiny-invariant";

export async function loader({ params }: Route.LoaderArgs) {
  //let chat = await fetchChat(params.chatId);
  return { chatId: params.chatId };
}

export async function action({ request, params }: Route.ActionArgs) {
  const formData = await request.formData();
  const message = formData.get("message");
  invariant(message, "Message is required");
  const stream = await updateChat({ message: message as string, id: params.chatId });

  if (!stream) {
    throw new Error("No stream returned from updateChat");
  }

  return new Response(stream.body, {
    headers: {
      "Content-Type": "text/plain",
      "X-Remix-Response": "true"
    },
  });
}

interface Message {
  id: string;
  content: string;
  role: "user" | "assistant";
}

const createUserMessage = (message: string) => {
  return {
    id: crypto.randomUUID(),
    content: message,
    role: "user",
  } as Message;
};

export default function Chat({ loaderData }: Route.ComponentProps) {
  const [messages, setMessages] = useState<Message[]>([]);
  const [currentMessage, setCurrentMessage] = useState<string>("");
  const fetcher = useFetcher();
  const pending = fetcher.state === "submitting";

  useEffect(() => {
    console.log("Resetting state", loaderData.chatId);
    setMessages([]);
    setCurrentMessage("");
  }, [loaderData.chatId]);

  useEffect(() => {
    if (fetcher.state === "idle" && fetcher.data) {
      const response = new Response(fetcher.data);
      const reader = response.body?.getReader();
      if (!reader) {
        console.error("No reader found");
        return;
      }

      // Create initial assistant message
      const assistantMessage: Message = {
        id: crypto.randomUUID(),
        content: "",
        role: "assistant",
      };
      setMessages(prev => [...prev, assistantMessage]);

      const decoder = new TextDecoder();

      const processStream = async () => {
        try {
          while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            const text = decoder.decode(value);
            console.log("got text", text);
            setMessages(prev => {
              const lastMessage = prev[prev.length - 1];
              if (lastMessage.role === "assistant") {
                return [
                  ...prev.slice(0, -1),
                  { ...lastMessage, content: lastMessage.content + text }
                ];
              }
              return prev;
            });
          }
        } catch (error) {
          console.error("Error processing stream:", error);
        }
      };

      processStream();
    }
  }, [fetcher.state, fetcher.data]);

  const createMessage = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (currentMessage.trim() === "") {
      return;
    }

    const userMessage = createUserMessage(currentMessage);
    setMessages([...messages, userMessage]);
    setCurrentMessage("");

    fetcher.submit(
      { message: currentMessage },
      { method: "post" }
    );
  };

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
                  message.role === "user" ? "bg-muted" : ""
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
          <form onSubmit={createMessage} className="flex gap-2 items-start">
            <textarea
              rows={3}
              placeholder="Type your message..."
              className="flex-1 resize-none rounded-2xl border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              value={currentMessage}
              onChange={(e) => setCurrentMessage(e.target.value)}
            />
            <Button
              variant="default"
              size="lg"
              type="submit"
              disabled={pending}
            >
              {/* {pending && <Loader2 className="w-4 h-4 animate-spin" />} */}
              Send
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
