import { type RouteConfig, index, prefix, route } from "@react-router/dev/routes";

export default [
  index("routes/home.tsx"),
  ...prefix("chats", [
    index("routes/chats/index.tsx"),
    //route("new", "routes/chats/new.tsx"),
    route(":chatId", "routes/chats/chat.tsx"),
  ]),
] satisfies RouteConfig;
