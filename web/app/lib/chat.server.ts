import "dotenv/config";

const apiUrl = process.env.API_URL;

async function updateChat({ id, message }: { id: string; message: string }) {
  const response = await fetch(`${apiUrl}/api/chat/${id}`, {
    method: "POST",
    headers: {
      Accept: "text/plain",
      "Content-Type": "application/json",
    },
    body: JSON.stringify(message), // api want the message as a json string
  });

  if (!response.ok) {
    console.error("response", response);
    throw new Error(
      `HTTP error! status: ${response.status} ${response.statusText}: ${response.body}`
    );
  }

  if (!response.body) {
    throw new Error("Response body is null");
  }

  return response.body;
}

export { updateChat };
