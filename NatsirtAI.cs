using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using OpenAI_API.Completions;
using OpenAI_API.Models;

namespace Natsirt;

public class NatsirtAI
{
    private readonly IConfiguration _configuration;
    private readonly OpenAIAPI _openAIAPI;

    public NatsirtAI(IConfiguration config)
    {
        _configuration = config;
        _openAIAPI = new OpenAIAPI(_configuration["OPENAI_API_KEY"]);
    }

    public async Task<string> PerformCompletion(string prompt)
    {

        var result = await _openAIAPI.Completions.CreateCompletionAsync(new CompletionRequest()
        {
            Prompt = prompt,
            MaxTokens = 100,
            Temperature = 0.5,
            TopP = 1,
            PresencePenalty = 0,
            FrequencyPenalty = 0,
            Model = Model.DavinciText,
            StopSequence = "\n",
        });
        
        return result.Completions[0].Text;
    }
}