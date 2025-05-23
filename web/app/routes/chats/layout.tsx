import { BotMessageSquare } from "lucide-react";
import { Link, Outlet, useLoaderData, useNavigate } from "react-router";
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
import { useState } from "react";
import { fetchAllChats } from "~/lib/chat.server";

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

interface Chat {
  id: string;
  name: string;
  createdAt: string;
}

function AppSidebar() {
  const { chats: initialChats } = useLoaderData<typeof loader>();
  const [chats, setChats] = useState<Chat[]>(initialChats);
  const navigate = useNavigate();

  function createChat() {
    const uuid = crypto.randomUUID();
    const chat = {
      id: uuid,
      name: `Chat ${new Date().toLocaleString()}`,
      createdAt: new Date().toISOString(),
    };
    console.log(chat);
    setChats([chat, ...chats]);
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
              {chats.map((chat) => (
                <SidebarMenuItem key={chat.id}>
                  <SidebarMenuButton
                    asChild
                    // isActive={location.pathname === "/"}
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
