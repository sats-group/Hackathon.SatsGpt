import { useEffect, useRef } from "react";
import ReactMarkdown from "react-markdown";
import type { ChatMessage } from "~/lib/chat-message";
import { cn } from "~/lib/utils";

export function Messages({ messages }: { messages: ChatMessage[] }) {
  const chatContainerRef = useRef<HTMLDivElement>(null);

  // biome-ignore lint/correctness/useExhaustiveDependencies: needs to happen when messages change
  useEffect(() => {
    if (chatContainerRef.current) {
      chatContainerRef.current.scrollTop =
        chatContainerRef.current.scrollHeight;
    }
  }, [messages]);

  return (
    <div className="flex-1 overflow-y-auto p-4 pb-12" ref={chatContainerRef}>
      <div className="mx-auto max-w-3xl space-y-4">
        {messages.length === 0 && (
          <div className="flex justify-center items-center h-[calc(100vh-24rem)]">
            <p className="text-muted-foreground text-xl font-semibold">
              Is there anything you'd like to know about SATS?
            </p>
          </div>
        )}
        {messages.map((message) => (
          <div
            key={message.id}
            className={cn(
              "flex w-full animate-in fade-in slide-in-from-bottom-2 duration-300",
              message.role === "user" ? "justify-end" : "justify-start"
            )}
          >
            <div
              className={cn(
                "max-w-[80%] rounded-lg px-4 py-2",
                message.role === "user" ? "bg-muted" : ""
              )}
            >
              <div className="prose">
                <ReactMarkdown>{message.content}</ReactMarkdown>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
