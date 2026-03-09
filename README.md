# Hacker News Consumer

Small .NET minimal API that fetches Hacker News "best stories" and returns story details. It uses an HttpClient-backed service, an in-memory cache (2 minutes) and limits concurrent requests when fetching individual items.

## What it does
- Calls the Hacker News API (`/beststories.json`) to get story IDs.
- Fetches each story (`/item/{id}.json`) in parallel with a concurrency limit.
- Caches the assembled list for 2 minutes to avoid hitting the external API on every request.
- Returns stories ordered by `score` (descending).

## Requirements
- .NET 10 SDK (project targets `net10.0`).
- Internet access to reach `https://hacker-news.firebaseio.com/v0` (unless you change configuration to a different base URL).

## Configuration
Configuration lives in `appsettings.json`.

Important keys:
- `HackerNewsApi:BaseUrl` — base URL for the Hacker News API (default in repo: `https://hacker-news.firebaseio.com/v0`).

Example `appsettings.json` excerpt:

```json
{
  "HackerNewsApi": {
    "BaseUrl": "https://hacker-news.firebaseio.com/v0"
  }
}
```

Note: Cache duration is currently implemented as a fixed 2 minutes inside `ApiConsumerService`. If you want it configurable, I can change it to read from configuration.

## Endpoints
- GET /stories?paginacao={n}
  - Returns a JSON list of stories. `paginacao` (query string) controls how many stories are returned (default is 10 when omitted).
  - Example: `GET /stories?paginacao=5`

The minimal API handler is defined in `Program.cs` and it resolves `IApiConsumerService` from DI; the service is implemented in `Services/ApiConsumerService.cs`.

## How to run locally
1. Build

```bash
dotnet build
```

2. Run

```bash
dotnet run --project ./hacker-news-consumer.csproj
```

3. Call the endpoint (replace host/port if different):

```bash
curl "http://localhost:5000/stories?paginacao=5"
```

If your environment binds HTTPS or different ports, check the output from `dotnet run` or `Properties/launchSettings.json`.

## Project structure (important files)
- `Program.cs` — minimal API configuration and route mapping.
- `Services/ApiConsumerService.cs` — logic that calls Hacker News, applies concurrency limits and in-memory caching.
- `Services/IApiConsumerService.cs` — service interface.
- `Dto/Story.cs` — DTO for Hacker News story objects.
- `appsettings.json` — configuration (HackerNewsApi:BaseUrl).

## Implementation notes and caveats
- The service uses `IMemoryCache` in-memory cache with a 2-minute TTL. This cache is per application instance (not distributed).
- Concurrency limit is implemented via `SemaphoreSlim(10)` in `ApiConsumerService`.
- The service currently fetches the first `paginacao` IDs returned by the `beststories.json` endpoint and then orders the fetched items by `score`. If you want the true top-N by score across all IDs, you should fetch more IDs (or all) and then pick the top-N by score.
- There's a nullable warning in `Dto/Story.cs` about a non-nullable property — you can address it by marking properties nullable or adding default initializers.

## Possible improvements (can implement on request)
- Make cache TTL configurable via `appsettings.json`.
- Add `CancellationToken` support through API handlers and service methods.
- Add Polly policies (retry/timeout/circuit-breaker) to the registered `HttpClient`.
- Cache individual stories to reuse across different `paginacao` values.
- Add logging to indicate cache hits/misses and HTTP failures for observability.

## Troubleshooting
- If you get build errors, confirm you have .NET 10 SDK installed:

```bash
dotnet --info
```

- If the external API is unreachable, the service will return an empty list or skip failed items according to the current error handling.

---
If you want, I can:
- Make the cache duration configurable;
- Add logs for cache hits/misses;
- Change the service to guarantee top-N by score globally (fetch more IDs before selecting top N).

Which of those should I implement next?