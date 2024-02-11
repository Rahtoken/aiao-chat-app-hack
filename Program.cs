using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;

var config = ParseConfig(null);
var openAIClient = new OpenAIClient(new Uri(config.AzureOpenAIEndpoint), new Azure.AzureKeyCredential(config.AzureOpenAIKey));

var scopes = new[] { "Notes.ReadWrite.All" };
var options = new DeviceCodeCredentialOptions
{
    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
    ClientId = config.ClientId,
    TenantId = config.TenantId,
    DeviceCodeCallback = (code, cancellation) =>
    {
        Console.WriteLine(code.Message);
        return Task.FromResult(0);
    },
};

var deviceCodeCredential = new DeviceCodeCredential(options);

var graphClient = new GraphServiceClient(deviceCodeCredential, scopes);

var sections = await graphClient.Me.Onenote.Sections.GetAsync();
Console.WriteLine("Which section do you want to practice?\n");
for (int i = 0; i < sections.Value.Count; ++i)
{
    Console.WriteLine($"{i + 1}. {sections.Value[i].DisplayName}");
}
Console.Write("Enter your selection: ");
int selection = Int32.Parse(Console.ReadLine());
string sectionId = sections.Value[selection - 1].Id;
var pages = await graphClient.Me.Onenote.Sections[sectionId].Pages.GetAsync();
string content = "";
foreach (var page in pages.Value)
{
    var pageContent = await graphClient.Me.Onenote.Pages[page.Id].Content.GetAsync();
    using var sr = new StreamReader(pageContent);
    content += sr.ReadToEnd();
}

var schema = new
{
    type = "object",
    properties = new
    {
        Questions = new
        {
            type = "array",
            items = new
            {
                type = "object",
                properties = new
                {
                    Question = new { type = "string" },
                    Answers = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    CorrectAnswer = new { type = "string" }
                },
                required = new[] { "question", "answers", "correctAnswer" }
            }
        }
    },
    required = new[] { "questions" }

};

var askQuestionsTool = new ChatCompletionsFunctionToolDefinition()
{
    Name = "ask_questions",
    Description = "Generate a list of 3 multiple-choice questions.",
    Parameters = BinaryData.FromObjectAsJson(schema,
        new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
};

var chatCompletionsOptions = new ChatCompletionsOptions()
{
    DeploymentName = "gpt-35-turbo",
    Messages = { new ChatRequestUserMessage("Ask questions based on: " + content) },
    Tools = { askQuestionsTool },
};

chatCompletionsOptions.ToolChoice = askQuestionsTool;

Response<ChatCompletions> response = await openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
var x = (response.Value.Choices[0].Message.ToolCalls[0] as ChatCompletionsFunctionToolCall).Arguments;
var jx = System.Text.Json.JsonSerializer.Deserialize<ResponseWrapper>(x);
foreach (var question in jx.Questions)
{
    Console.WriteLine(question.Question);
    for (int i = 0; i < question.Answers.Count; ++i)
    {
        Console.WriteLine($"{i + 1}. {question.Answers[i]}");
    }
    Console.Write("Select your answer: ");
    if (question.CorrectAnswer == question.Answers[Int32.Parse(Console.ReadLine()) - 1])
    {
        Console.WriteLine("Correct! ✅");
    }
    else
    {
        Console.WriteLine("Wrong ❌");
    }
}



Config ParseConfig(string? configPath)
{
    if (configPath == null)
    {
        configPath = $"{AppDomain.CurrentDomain.BaseDirectory}config.json";
    }

    return System.Text.Json.JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
}

async Task<string> GetToken(Config config)
{
    IConfidentialClientApplication app;
    app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                                              .WithClientSecret(config.ClientSecret)
                                              .WithAuthority(new Uri($"https://login.microsoftonline.com/{config.TenantId}"))
                                              .Build();
    string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
    return (await app.AcquireTokenForClient(scopes)
                  .ExecuteAsync()).AccessToken;
}

public record Config(
    [property: JsonPropertyName("azureOpenAIKey")] string AzureOpenAIKey,
    [property: JsonPropertyName("azureOpenAIEndpoint")] string AzureOpenAIEndpoint,
    [property: JsonPropertyName("clientId")] string ClientId,
    [property: JsonPropertyName("clientSecret")] string ClientSecret,
    [property: JsonPropertyName("tenantId")] string TenantId);

public record ResponseWrapper(
    [property: JsonPropertyName("questions")] List<QuestionRecord> Questions
);

public record QuestionRecord(
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("answers")] List<string> Answers,
    [property: JsonPropertyName("correctAnswer")] string CorrectAnswer
);