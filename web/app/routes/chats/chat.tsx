import { Button } from "~/components/ui/button";
import type { Route } from "./+types/chat";
import { cn } from "~/lib/utils";
import { useState, useEffect } from "react";
import { Loader2 } from "lucide-react";
import { fetchChat } from "~/lib/chat.server";
import { useFetcher } from "react-router";
import type { ChatMessage, Chat } from "~/lib/chat-message";
import { useChatStore } from "~/lib/chat-store";

export async function loader({ params }: Route.LoaderArgs) {
  const chat = await fetchChat({ id: params.chatId });

  if (!chat) {
    return {
      chat: {
        id: params.chatId,
        name: `Chat ${params.chatId}`,
        createdAt: new Date().toISOString(),
        messages: [],
      },
    };
  }

  return { chat };
}



const createUserMessage = (message: string) => {
  return {
    content: message,
    role: "user",
    id: crypto.randomUUID(),
  } as ChatMessage;
};

export default function ChatView({ loaderData, params }: Route.ComponentProps) {
  const { chat } = loaderData;
  const [messages, setMessages] = useState<ChatMessage[]>(chat.messages);
  const [currentMessage, setCurrentMessage] = useState<string>("");
  const [isLoading, setIsLoading] = useState(false);
  const updateChatName = useChatStore((state) => state.updateChatName);
  const fetcher = useFetcher();

  useEffect(() => {
    setMessages(chat.messages);

    // Summarize chat when opened if it has messages
    // if (chat.messages.length > 0 && chat.name.startsWith("Untitled")) {
    //   console.log("Summarizing chat", chat.id);
    //   fetcher.submit(null, {
    //     method: "POST",
    //     action: `/chats/${chat.id}/summarize`,
    //   });
    // }
  }, [chat, fetcher]);

  useEffect(() => {
    if (fetcher.data?.name) {
      updateChatName(chat.id, fetcher.data.name);
    }
  }, [fetcher.data, chat.id, updateChatName]);

  const handleStreamResponse = async (response: Response) => {
    if (!response.body) {
      throw new Error("Response body is null");
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder();

    // Create initial assistant message
    const assistantMessage: ChatMessage = {
      content: "",
      role: "assistant",
      id: crypto.randomUUID(),
    };
    setMessages((prev) => [...prev, assistantMessage]);

    try {
      while (true) {
        const { done, value } = await reader.read();
        if (done) {
          break;
        }

        const text = decoder.decode(value);
        setMessages((prev) => {
          const lastMessage = prev[prev.length - 1];
          if (lastMessage.role === "assistant") {
            return [
              ...prev.slice(0, -1),
              { ...lastMessage, content: lastMessage.content + text },
            ];
          }
          return prev;
        });
      }
    } catch (error) {
      console.error("Error processing stream:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const createMessage = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (currentMessage.trim() === "" || isLoading) {
      return;
    }

    setIsLoading(true);
    const userMessage = createUserMessage(currentMessage);
    setMessages((prev) => [...prev, userMessage]);
    setCurrentMessage("");

    try {
      const response = await fetch(`/chats/${loaderData.chat.id}/stream`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(currentMessage),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      await handleStreamResponse(response);
    } catch (error) {
      console.error("Error sending message:", error);
      setIsLoading(false);
    }
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
              disabled={isLoading}
            />
            <Button
              variant="default"
              size="lg"
              type="submit"
              disabled={isLoading}
            >
              {isLoading && <Loader2 className="w-4 h-4 mr-2 animate-spin" />}
              Send
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
