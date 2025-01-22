using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.Ollama;
using Microsoft.SemanticKernel;
using SemanticKernelDemo.Database;

namespace SemanticKernelDemo.SemanticKernel;

public static class SemanticKernelExtensions
{
    private const string AzureOpenAiEndpoint = "**Your Azure OpenAI endpoint**";
    private const string AzureOpenAiApiKey = "**Your Azure OpenAI API key**";
    private const string AzureOpenAiChatCompletionDeployment = "**Your Azure OpenAI deployment**";
    private const string AzureOpenAiEmbeddingDeployment = "**Your Azure OpenAI deployment**";


    


    public static void AddSemanticKernel(this WebApplicationBuilder builder)
    {


        //var config1 = new OllamaConfig
        //{
        //    Endpoint = "http://127.0.0.1:11434/",
        //    TextModel = new OllamaModelConfig("llama3:latest", 131072),
        //    EmbeddingModel = new OllamaModelConfig("nomic-embed-text", 2048)
        //};
        //var logLevel = LogLevel.Warning;
        //var memory1 = new KernelMemoryBuilder()
        //    .WithOllamaTextGeneration(config1)
        //    .WithOllamaTextEmbeddingGeneration(config1)
        //    .Configure(builder => builder.Services.AddLogging(l =>
        //    {
        //        l.SetMinimumLevel(logLevel);
        //        l.AddSimpleConsole(c => c.SingleLine = true);
        //    }))
        //    .Build();

        //// Import some text
        //var d = memory1.ImportTextAsync("Today is October 32nd, 2476").Result;

        //// Generate an answer - This uses OpenAI for embeddings and finding relevant data, and LM Studio to generate an answer
        //var answer = memory1.AskAsync("What's the current date (don't check for validity)?").Result;
        //Console.WriteLine("-------------------");
        //Console.WriteLine(answer.Question);
        //Console.WriteLine(answer.Result);
        //Console.WriteLine("-------------------");












        var endpoint = new Uri("http://127.0.0.1:11434/");
        var modelId = "llama3:latest";

        var config = new OllamaConfig
        {
            Endpoint = "http://127.0.0.1:11434/",
            TextModel = new OllamaModelConfig("llama3:latest", 131072)
            ,EmbeddingModel = new OllamaModelConfig("nomic-embed-text", 2048)
        };
        Uri uri = new Uri("http://127.0.0.1:11434/");
        var memory = new KernelMemoryBuilder()
            .WithOllamaTextGeneration(config)
            .WithOllamaTextEmbeddingGeneration(config).WithSearchClientConfig(new()
            {
                EmptyAnswer =
                    "I'm sorry, I haven't found any relevant information that can be used to answer your question",
                MaxMatchesCount = 25,
                AnswerTokens = 800
            })
            .WithCustomTextPartitioningOptions(new()
            {
                // Defines the properties that are used to split the documents in chunks.
                MaxTokensPerParagraph = 1000,
                MaxTokensPerLine = 300,
                OverlappingTokens = 100
            })
            .WithPostgresMemoryDb(new PostgresConfig
            {
                ConnectionString = DbConstants.ConnectionString
            })
            .Build();

        var kernelBuilder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        kernelBuilder.Services.AddOllamaTextGeneration(modelName: modelId, endpoint: "http://127.0.0.1:11434/");
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var kernel = kernelBuilder.Build();

        var plugin = new MemoryPlugin(memory, "kernelMemory");
        kernel.ImportPluginFromObject(plugin, "memory");

        builder.Services.AddSingleton(kernel);
        builder.Services.AddSingleton(memory);
        builder.Services.AddTransient<ISemanticKernelService, SemanticKernelService>();
    }
}
