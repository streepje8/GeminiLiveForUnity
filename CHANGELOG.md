# Version 1.0.0
The first release of the gemini live for unity api

- Added Sessions
- Added Live Supported Models (BidiGenerateContent) as of 23/10/2025
- Added Speech Configuration Support with multiple voices
- Added Thinking Configuration Support
- Added Sliding Context Window 

# Version 1.0.1
New events and stabilty changes

- Move AsyncDispatcher to main library
- Simplified AsyncDispatcher
- Improved CancellationToken Flow
- Async Event handlers will now also get Cancellation Tokens
- New Event 'GeminiGenerationCompleteEvent' (When generation finishes)
- New Event 'GeminiTurnEndEvent' (When the AI finishes talking)
- Usage Metrics Support
- New Event 'GeminiReceiveUsageMetricsEvent' (When Usage Metrics are Received)
- Stability Changes