import { type RouteConfig, index, layout, prefix, route } from "@react-router/dev/routes";

export default [
  index("routes/home.tsx"),
  layout("routes/chats/layout.tsx", [
    route("chats", "routes/chats/index.tsx"),
    //route("new", "routes/chats/new.tsx"),
    route("chats/:chatId", "routes/chats/chat.tsx"),
    route("chats/:chatId/stream", "routes/chats/stream.tsx"),
  ]),
] satisfies RouteConfig;
