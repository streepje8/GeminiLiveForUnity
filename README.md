---

# 🌌 Gemini Live Integration for Unity

Integrates Google’s **Gemini Live WebSocket API** seamlessly into **Unity** projects using C#.
Currently, **text input** is supported, while both **text and audio outputs** are available. Audio output is streamed in real time via WebSocket.

---

## ⚠️ Disclaimer

This project is **not affiliated, associated, authorized, endorsed by, or in any way officially connected with Google LLC**, its subsidiaries, or affiliates.
All product names, trademarks, and registered trademarks mentioned in this project are the property of their respective owners.
This integration simply provides a Unity-compatible interface for interacting with the **public Gemini Live API**.

---
## ✨ Features

* 🎯 **Multiple Model Support** (see list below)
* 🗣️ **Text and Audio Modalities**
* 🌍 **Multiple Language Codes**
* 🎵 **Multiple Voices**
* 🧠 **Configurable Thinking Mode**
* 🔊 **Output and Input Transcriptions** (input transcription available for text only, for now)
* 🪞 **Sliding Context Window**
* ⚙️ **Custom System Instructions**
* 🧼 **Basic and Extendable Sanitization**
* 📏 **Adjustable Output Parameters:**

  * Max Output Tokens
  * Temperature
  * Top-P
  * Top-K
* ⚡ **Asynchronous API**

---

## 🧩 Supported Models

| Model Name                           | Description                              |
| ------------------------------------ | ---------------------------------------- |
| Flash 2.0 Experimental               | Early experimental version for testing   |
| Flash 2.0 Live                       | Live model for real-time use             |
| Flash 2.5 Preview                    | Preview release of Flash 2.5             |
| Flash 2.5 Live Preview               | Live preview of Flash 2.5                |
| Flash 2.5 Native Audio Latest        | Latest version with native audio support |
| Flash 2.5 Native Audio (ver 09-2025) | Specific version from September 2025     |

---

## 🚀 Getting Started

1. **Acquire an API Key**
   Visit [https://aistudio.google.com/api-keys](https://aistudio.google.com/api-keys) and aquire a **Gemini API key**.

2. **Add the Package to Unity**

   * Open your Unity project.
   * Go to **Window → Package Manager**.
   * Click the **‘+’** button in the top-left corner and select **“Add package from Git URL…”**
   * Enter the following URL:

     ```
     https://github.com/streepje8/GeminiLiveForUnity.git
     ```
   * Click **Add** and wait for Unity to import and compile the package.

3. **Try the Sample Scene**

   * Open the **Sample Scene** provided with the package.
   * Select the **Gemini GameObject** in the scene.
   * Paste your **API Key** into the Gemini component’s field.
   * Run the scene to test text and audio outputs.

---

## 🧠 Samples

The package includes a **Script Usage Sample** demonstrating how to interact with the Gemini Live API from code.
Use it as a foundation for your own integrations or prototypes. To get started, locate the Gemini object in the sample scene and paste in your API key.

---
## 🧩 Session Events

The Gemini Live for Unity integration provides several session-related events that allow you to track the state and activity of a Gemini session in real time.

| **Event Name** | **Description** |
|-----------------|-----------------|
| `GeminiSessionStartEvent` | Triggered when a new Gemini session is **started**. |
| `GeminiSessionReadyEvent` | Triggered when the Gemini session is **ready to receive prompts**. |
| `GeminiSessionEndEvent` | Triggered when the Gemini session has **ended**. |
| `GeminiSessionInteractionEvent` | Triggered when **Gemini generates an interaction or response** during the session. |
| `GeminiPromptedEvent` | Triggered when a **user sends input or a prompt** to Gemini. |
| `GeminiTranscribeEvent` | Triggered when a **transcription** (by Gemini) is received. |
| `GeminiServerClosedSessionEvent` | Triggered when **Gemini’s server closes the session** — exceptions are thrown instead if this happens due to a client error. |
| `GeminiJsonPacketReceived` | Triggered when **raw JSON data** is received from the Gemini WebSocket. |
| `GeminiLiveEvent` | A **catch-all event** that fires for *any* Gemini-related event in the session. |

> 💡 Use these events via `Session.AddListener<>()` and `Session.RemoveListener<>()` to hook your own scripts up to the gemini session.
---

## 🗺️ Future Roadmap

Planned and potential future enhancements include:

* 🎤 **Audio Input**
* 🔁 **Partial Configuration Updates** (without restarting sessions)

If you’d like to contribute or take on one of these features:

* 💡 **Open a pull request** for feature contributions.
* 🧾 **Submit an issue** for suggestions or bug reports.

---

## 💬 Contributing

Contributions are very welcome!
Please fork the repository, create a feature branch, and submit a pull request with a clear description of your changes.

---

## 📄 License  

This project is licensed under the BSL license — see the LICENSE.md file for more details.  
  
In short: For personal use you can use it for free, for commercial use ask me first. When in doubt, ask.  
After 01/01/2028 the license changes to MIT.

---

## 💖 Acknowledgements

This project was **generously funded by [Dylan from Dunk Technologies](https://www.linkedin.com/in/smitdylan)**.
Definitely check them out — their support made this integration possible!

---