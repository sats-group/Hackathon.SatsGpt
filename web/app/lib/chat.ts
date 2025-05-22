import "dotenv/config";

const apiUrl = process.env.API_URL;

async function fetchChat(chatId: string) {
  const response = await fetch(`${apiUrl}/api/chats/${chatId}`);
  return response.json();
}

async function createChat() {
  const response = await fetch(`${apiUrl}/api/chats`, {
    method: "POST",
  });
  return response.json();
}

export { fetchChat, createChat };
