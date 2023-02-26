using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Natsirt;

public class NatsirtCave
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private SocketTextChannel _channel;

    public NatsirtCave(DiscordSocketClient client, InteractionService handler, IConfiguration config)
    {
        _client = client;
        _configuration = config;

        _client.Ready += ReadyAsync;
    }

    private async Task ReadyAsync()
    {
        _channel = _client.GetGuild(ulong.Parse(_configuration["GUILD_ID"]))
            .GetTextChannel(ulong.Parse(_configuration["CAVE_CHANNEL_ID"]));
    }

    public async void SendToCave(IMessage message)
    {
        await _channel.SendMessageAsync(message.Content);
    }

    public async void SendToCave(string message)
    {
        await _channel.SendMessageAsync(message);
    }

    public async void SendToCave(Embed message)
    {
        await _channel.SendMessageAsync(embed: message);
    }

    public async void SendErrorToCave(string message)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(message);
        writer.Flush();
        stream.Position = 0;

        var file = new FileAttachment(stream, "error.txt");


        await _channel.SendFileAsync(file, "Error");
    }
}