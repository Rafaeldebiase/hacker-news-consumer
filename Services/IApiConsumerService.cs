using HackerNews.Dto;

namespace HackerNews.Services
{
    public interface IApiConsumerService
    {
        Task<List<Story>> GetTopStoriesAsync(int paginacao);
    }
}
