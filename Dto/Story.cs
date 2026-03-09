namespace HackerNews.Dto
{
    public record Story
    {
        public string By { get; set; }
        public int Descendants { get; set; }
        public int Id { get; set; }
        public List<int>? Kids { get; set; }
        public int Score { get; set; }
        public string? Text { get; set; }
        public long Time { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
    }
}