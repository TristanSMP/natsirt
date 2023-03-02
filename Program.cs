using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Natsirt.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Natsirt;

public class Program
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    private readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent | GatewayIntents.GuildMessages | GatewayIntents.GuildVoiceStates,
        AlwaysDownloadUsers = true,
    };

    public Program()
    {
        _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables("DC_")
            .Build();

        _services = new ServiceCollection()
            .AddSingleton(_configuration)
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<AdminAPI>()
            .AddSingleton<NatsirtCave>()
            .AddSingleton<NatsirtAI>()
            .AddSingleton<VCGroups>()
            .AddSingleton<TicketLogs>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .BuildServiceProvider();
    }

    private static void Main(string[] args)
    {
        new Program().RunAsync()
            .GetAwaiter()
            .GetResult();
    }

    public async Task RunAsync()
    {
        var client = _services.GetRequiredService<DiscordSocketClient>();

        client.Log += LogAsync;
        
        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();
        
        await _services.GetRequiredService<VCGroups>()
            .InitializeAsync();
        
        await _services.GetRequiredService<TicketLogs>()
            .InitializeAsync();

        await client.LoginAsync(TokenType.Bot, _configuration["token"]);
        await client.StartAsync();
        await client.SetActivityAsync(new StreamingGame("tsmp's events", "https://www.twitch.tv/twisttaan"));

        await Task.Delay(Timeout.Infinite);
    }

    private async Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
            return false;
#endif
    }
}