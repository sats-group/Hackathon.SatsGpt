🔐 Authentication in Member Apps

🧠 Overview

The document outlines two authentication methods used by SATS member applications:
	1.	Cookie-based authentication – Currently marked as TODO.
	2.	One-time token key authentication – A secure method for enabling authenticated web access from within the app.

⸻

🔑 One-Time Token Key Authentication

This method allows an already authenticated app user to obtain a temporary, single-use token for accessing the BFF (Backend for Frontend) in a browser context—ideal for implementing SSO-style flows.

Use Case Example

Sending a user from the mobile app to a web view while maintaining authenticated state (e.g., navigating to a member profile page).

⸻

📋 How It Works
	1.	Create a one-time key
	•	The client (mobile app) sends an authenticated POST request to the BFF endpoint:

POST /one-time-token-key/create


	•	The response includes a secure one-time key:

{
  "key": "some-secure-key"
}


	2.	Redirect the user to the browser with the key
	•	Construct a URL like:

BFF/weblinks/memberprofiling?one-time-token-key=<url_encoded_key>


	•	Open the client’s browser to this URL.

	3.	BFF handles the authentication
	•	It responds with 302 Found and a Location header pointing to the actual web destination.
	•	The real authentication token is passed in the redirect URL as a query parameter.

⸻

⚠️ Notes
	•	The key must be URL-encoded when included in the query string.
	•	This token is valid only once and expires shortly after creation.