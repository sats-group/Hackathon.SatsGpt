import { BotMessageSquare } from "lucide-react";
import { Link, Outlet, useLoaderData, useNavigate } from "react-router";
import { useEffect, useRef } from "react";
import { Button } from "~/components/ui/button";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
} from "~/components/ui/sidebar";
import { SidebarInset } from "~/components/ui/sidebar";
import { fetchAllChats } from "~/lib/chat.server";
import { useChatStore, useAllChats } from "~/lib/chat-store";
import type { Chat } from "~/lib/chat-message";

export async function loader() {
  const chats = await fetchAllChats();
  return { chats };
}

export default function Layout() {
  return (
    <SidebarProvider>
      <AppSidebar />
      <SidebarInset>
        <Outlet />
        {/* <TailwindIndicator /> */}
      </SidebarInset>
    </SidebarProvider>
  );
}

function AppSidebar() {
  const { chats: downloadedChats } = useLoaderData<typeof loader>();
  const replaceChats = useChatStore((state) => state.replaceChats);
  const allChats = useAllChats();
  const initialLoadDone = useRef(false);

  useEffect(() => {
    if (!initialLoadDone.current) {
      replaceChats(downloadedChats);
      initialLoadDone.current = true;
    }
  }, [downloadedChats, replaceChats]);

  const addChat = useChatStore((state) => state.addChat);
  const navigate = useNavigate();

  function createChat() {
    const uuid = crypto.randomUUID();
    const chat = {
      id: uuid,
      name: `Chat ${new Date().toLocaleString()}`,
      createdAt: new Date().toISOString(),
      messages: [],
    } as Chat;
    console.log(chat);
    addChat(chat);
    navigate(`/chats/${uuid}`);
  }

  return (
    <Sidebar>
      <SidebarHeader className="text-2xl font-bold text-center">
        SATS GPT
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              <SidebarMenuItem>
                {/* <SidebarMenuButton
                    asChild
                    size="lg"
                    variant="outline"
                    // isActive={location.pathname === "/"}
                  > */}
                <Button onClick={() => createChat()} className="w-full">
                  <BotMessageSquare />
                  <span>New Chat</span>
                </Button>
                {/* </SidebarMenuButton> */}
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
        <SidebarGroup>
          <SidebarGroupLabel>History</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {allChats.map((chat) => (
                <SidebarMenuItem key={chat.id}>
                  <SidebarMenuButton
                    asChild
                    isActive={location.pathname === `/chats/${chat.id}`}
                  >
                    <Link to={`/chats/${chat.id}`}>
                      {chat.name}
                      {/* <span className="text-xs text-gray-500">
                        {new Date(chat.createdAt).toLocaleString()}
                      </span> */}
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}
