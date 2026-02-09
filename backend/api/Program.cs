using Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Scalar.AspNetCore;
using StackExchange.Redis;
using ToolsPack.Logging;

var builder = WebApplication.CreateBuilder(args);

var modelId = builder.Configuration.GetValue<string>("ModelId") ?? throw new InvalidOperationException("Missing ModelId config");
var apiKey = builder.Configuration.GetValue<string>("ApiKey") ?? throw new InvalidOperationException("Missing ApiKey config");
var redisServer = builder.Configuration.GetValue<string>("RedisServer") ?? throw new InvalidOperationException("Missing RedisServer config");

builder.Services.AddLogging(services => 
{
    services.AddConsole();
    services.SetMinimumLevel(LogLevel.Trace); // Trace shows the actual prompts
});
builder.Services.AddTransient<CompactHttpLoggingMiddleware>();
builder.Services.AddHttpClient("OpenAPI").AddHttpMessageHandler<CompactHttpLoggingMiddleware>();
// Use a temporary service provider to grab the client while building
var openApiHttpClient = builder.Services.BuildServiceProvider()
    .GetRequiredService<IHttpClientFactory>()
    .CreateClient("OpenAPI");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisServer));
builder.Services.AddSingleton<ILightRepository, RedisLightRepository>();
builder.Services.AddKernel()
    .AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey, httpClient: openApiHttpClient)
    .Plugins.AddFromType<LightsPlugin>("Lights");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Enable planning
OpenAIPromptExecutionSettings openAiPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

Dictionary<Guid, ChatHistory> chatHistoryStorage = new();

app.MapPost("/chat/session", () =>
    {
        var history = new ChatHistory();
        var id = Guid.NewGuid();
        chatHistoryStorage.Add(id, history);
        return id.ToString();
    })
    .WithSummary("Create a new chat session")
    .WithDescription("Return the id of the chat session. The session is purged when server restart.");

app.MapPost("/chat/{session:guid}/message",
        async (
            [FromServices]Kernel kernel, [FromServices]IChatCompletionService chatCompletionService,
            [FromRoute] Guid session, HttpRequest request) =>
        {
            if (!chatHistoryStorage.TryGetValue(session, out var chatHistory))
            {
                throw new ArgumentException($"The chat session {session} was not found.");
            }
            using var reader = new StreamReader(request.Body);
            string message = await reader.ReadToEndAsync();
            chatHistory.AddUserMessage(message);
            var result = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: openAiPromptExecutionSettings,
                kernel: kernel);
            var response = result.Content ?? string.Empty;
            chatHistory.AddMessage(result.Role, response);
            return response;
        }
    )
    .Accepts<string>("text/plain")
    .WithSummary("Send a message, get a response")
    .WithDescription("Send a new message to a specific chat session and Get the entire response.");

app.MapPost("/chat/{session:guid}/array",
        async ([FromServices]Kernel kernel, [FromServices]IChatCompletionService chatCompletionService,
            [FromRoute] Guid session, HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            string message = await reader.ReadToEndAsync();
            return StreamChat(kernel, chatCompletionService, session, message);
        }
    )
    .Accepts<string>("text/plain")
    .WithSummary("Send a message, get response's chunk as array of string")
    .WithDescription("Send a new message to a specific chat session and Get the entire response as array of strings.");

app.MapPost("/chat/{session:guid}/sse",
        async (
            [FromServices]Kernel kernel, [FromServices]IChatCompletionService chatCompletionService,
            [FromRoute] Guid session, HttpRequest request, HttpContext context) =>
        {
            if (!chatHistoryStorage.TryGetValue(session, out var chatHistory))
            {
                throw new ArgumentException($"The chat session {session} was not found.");
            }

            using var reader = new StreamReader(request.Body);
            string message = await reader.ReadToEndAsync();
            chatHistory.AddUserMessage(message);

            var streamMessageContent = chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatHistory,
                executionSettings: openAiPromptExecutionSettings,
                kernel: kernel);
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

            await foreach (var chunk in streamMessageContent)
            {
                await context.Response.WriteAsync($"data: {chunk.Content}\n\n");
                await context.Response.Body.FlushAsync();
            }
        }
    )
    .Accepts<string>("text/plain")
    .WithSummary("Send a message, get response SSE stream")
    .WithDescription("Send a new message to a specific chat session and Get the response stream via SSE");

app.Run();


// Controller action
async IAsyncEnumerable<string> StreamChat(
    Kernel kernel, IChatCompletionService chatCompletionService,
    Guid session, string message)
{
    if (!chatHistoryStorage.TryGetValue(session, out var chatHistory))
    {
        throw new ArgumentException($"The chat session {session} was not found.");
    }

    chatHistory.AddUserMessage(message);

    var streamMessageContent = chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatHistory,
        executionSettings: openAiPromptExecutionSettings,
        kernel: kernel);

    await foreach (var chunk in streamMessageContent)
    {
        if (!chunk.Role.HasValue) continue;
        var response = chunk.Content ?? "";
        chatHistory.AddMessage(chunk.Role.Value, response);
        yield return response;
    }
}