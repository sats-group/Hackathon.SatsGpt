import { create } from "zustand";
import type { Chat } from "./chat-message";

interface ChatStore {
  chats: Chat[];
  localChats: Chat[];
  replaceChats: (chats: Chat[]) => void;
  updateChatName: (chatId: string, name: string) => void;
  addChat: (chat: Chat) => void;
  syncChat: (chat: Chat) => void;
}

export const useChatStore = create<ChatStore>((set) => ({
  chats: [],
  localChats: [],
  replaceChats: (chats: Chat[]) => {
    set((state) => ({
      chats,
      // Keep local chats that aren't in the server response
      localChats: state.localChats.filter(
        (localChat) => !chats.some((chat) => chat.id === localChat.id)
      ),
    }));
  },
  addChat: (chat: Chat) => {
    set((state) => ({
      localChats: [chat, ...state.localChats],
    }));
  },
  syncChat: (chat: Chat) => {
    set((state) => ({
      chats: [chat, ...state.chats],
      localChats: state.localChats.filter((c) => c.id !== chat.id),
    }));
  },
  updateChatName: (chatId: string, name: string) => {
    set((state) => ({
      chats: state.chats.map((chat) =>
        chat.id === chatId ? { ...chat, name } : chat
      ),
      localChats: state.localChats.map((chat) =>
        chat.id === chatId ? { ...chat, name } : chat
      ),
    }));
  },
}));

// Create a memoized selector for allChats
export const useAllChats = () => {
  const chats = useChatStore((state) => state.chats);
  const localChats = useChatStore((state) => state.localChats);

  return [...localChats, ...chats.filter(chat => !localChats.some(local => local.id === chat.id))];
};