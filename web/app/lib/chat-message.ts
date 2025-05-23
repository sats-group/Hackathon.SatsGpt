
export interface ChatMessage {
  content: string;
  role: "user" | "assistant";
  id: string;
}

export  interface Chat {
  id: string;
  name: string;
  createdAt: string;
  messages: ChatMessage[];
}