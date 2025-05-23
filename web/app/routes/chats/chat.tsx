import { Button } from "~/components/ui/button";
import type { Route } from "./+types/chat";
import { cn } from "~/lib/utils";
import { useState, useEffect, useRef } from "react";
import { ArrowUp, CircleArrowUp, Loader2 } from "lucide-react";
import { fetchChat } from "~/lib/chat.server";
import { useFetcher } from "react-router";
import type { ChatMessage, Chat } from "~/lib/chat-message";
import { useChatStore } from "~/lib/chat-store";
import ReactMarkdown from "react-markdown";
import { Messages } from "./components/messages";

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
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    setMessages(chat.messages);

    // Summarize chat when opened if it has messages
    const summaryEnabled = false;
    if (
      summaryEnabled &&
      chat.messages.length > 0 &&
      chat.name.startsWith("Untitled")
    ) {
      console.log("Summarizing chat", chat.id);
      fetcher.submit(null, {
        method: "POST",
        action: `/chats/${chat.id}/summarize`,
      });
    }
  }, [chat, fetcher]);

  useEffect(() => {
    if (fetcher.data?.name) {
      updateChatName(chat.id, fetcher.data.name);
    }
  }, [fetcher.data, chat.id, updateChatName]);

  // biome-ignore lint/correctness/useExhaustiveDependencies: needs to happen when messages change
  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.focus();
    }
  }, [isLoading, textareaRef]);

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

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      const form = e.currentTarget.form;
      if (form) {
        form.requestSubmit();
      }
    }
  };

  return (
    <div className="flex h-[calc(100vh)] flex-col">
      <Messages messages={messages} />
      <div className="p-4 pb-18 pt-0">
        <div className="mx-auto max-w-3xl border rounded-2xl p-2 shadow-lg ring-offset-background focus-within:ring-1 focus-within:ring-ring">
          <form onSubmit={createMessage} className="flex gap-2 items-end">
            <textarea
              rows={3}
              placeholder="Ask about SATS…"
              className="w-full resize-none rounded-xl bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus-visible:outline-none"
              value={currentMessage}
              onChange={(e) => setCurrentMessage(e.target.value)}
              onKeyDown={handleKeyDown}
              disabled={isLoading}
              ref={textareaRef}
            />
            <Button
              variant="default"
              size="icon"
              type="submit"
              className="rounded-full cursor-pointer mr-2 mb-2 ml-1"
              disabled={isLoading}
            >
              {isLoading && <Loader2 className="w-8 h-8 animate-spin" />}
              {!isLoading && <ArrowUp className="w-8 h-8" />}
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
