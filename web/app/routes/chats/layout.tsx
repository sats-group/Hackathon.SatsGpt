import { Outlet } from  "react-router";

export default function Layout() {
  return (
    <div>
      <h1>Chats</h1>
      <Outlet />
    </div>
  );
}
