import { BotMessageSquare } from "lucide-react";
import { Link, Outlet } from "react-router";
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

  return (
    <Sidebar>
      <SidebarHeader className="text-2xl font-bold text-center">
        SATS GPT
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Chats</SidebarGroupLabel>
          <SidebarGroupContent>
            {/* <Button variant="default">New Chat</Button>
            <Link to="/chats">Chats</Link> */}
            <SidebarMenu>
                <SidebarMenuItem>
                  <SidebarMenuButton
                    asChild
                    size="lg"
                    variant="outline"
                    // isActive={location.pathname === "/"}
                  >
                    <Link to="/chats/new">
                      <BotMessageSquare />
                      <span>New Chat</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}
