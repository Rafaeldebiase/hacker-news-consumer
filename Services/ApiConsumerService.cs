using HackerNews.Dto;
using HackerNews.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNews.Services
{
    public class ApiConsumerService : IApiConsumerService
    {
    private readonly HttpClient _httpClient;
    private readonly HackerNewsApiOptions _options;
    private readonly IMemoryCache _cache;

        public ApiConsumerService(HttpClient httpClient, IOptions<HackerNewsApiOptions> options, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _cache = cache;
        }

        public async Task<List<Story>> GetTopStoriesAsync(int pageSize)
        {
            var cacheKey = $"topstories:{pageSize}";
            if (_cache.TryGetValue<List<Story>>(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            var baseUrl = _options.BaseUrl?.TrimEnd('/') ?? string.Empty;

            // obtém lista de ids
            var topIds = await _httpClient.GetFromJsonAsync<List<int>>($"{baseUrl}/beststories.json");
            if (topIds == null || topIds.Count == 0)
            {
                return new List<Story>();
            }

            var ids = topIds.Take(pageSize);

            // limite de concorrência para não abrir muitas conexões simultâneas
            using var semaphore = new SemaphoreSlim(10);

            var tasks = ids.Select(async id =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // busca cada item, tolerando falhas
                    return await _httpClient.GetFromJsonAsync<Story?>($"{baseUrl}/item/{id}.json");
                }
                catch
                {
                    return null;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);

            // filtra nulos e ordena por score
            var final = results
                .Where(s => s != null)
                .Select(s => s!)
                .OrderByDescending(s => s.Score)
                .ToList();

            // cache por 2 minutos
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };
            _cache.Set(cacheKey, final, cacheOptions);

            return final;
        }
    }
}