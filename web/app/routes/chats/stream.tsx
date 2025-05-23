import type { Route } from "./+types/stream";
import { updateChat } from "~/lib/chat.server";
import invariant from "tiny-invariant";

export async function action({ request, params }: Route.ActionArgs) {
  const message = await request.json();
  invariant(message, "Message is required");
  const stream = await updateChat({ message: message as string, id: params.chatId });

  if (!stream) {
    throw new Error("No stream returned from updateChat");
  }

  // // Clone the stream for inspection
  // const [stream1, stream2] = stream.tee();
  // const reader = stream2.getReader();
  // const decoder = new TextDecoder();

  // // Log each chunk as it comes in
  // function readChunks() {
  //   reader.read().then(({ done, value }: { done: boolean; value: Uint8Array }) => {
  //     if (value) {
  //       const text = decoder.decode(value);
  //       console.log("chunk:", done, text);
  //     }
  //     if (!done) {
  //       readChunks();
  //     }
  //   });
  // }
  // readChunks();

  return new Response(stream, {
    headers: {
      "Content-Type": "text/plain",
      "X-Remix-Response": "true",
      //"Transfer-Encoding": "chunked",
    },
  });
}