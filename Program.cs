using HackerNews.Options;
using HackerNews.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<HackerNewsApiOptions>(builder.Configuration.GetSection("HackerNewsApi"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IApiConsumerService, ApiConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/stories", async (IApiConsumerService api, int? paginacao) =>
{
    var pageSize = paginacao ?? 10;
    var stories = await api.GetTopStoriesAsync(pageSize);
    return Results.Ok(stories);
}).WithName("GetStories");


app.Run();

