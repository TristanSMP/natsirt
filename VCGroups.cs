using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Natsirt;

public class VCGroups
{
    private readonly DiscordSocketClient _client;
    private readonly ulong _createChannelId = 1080608235364552774;
    private readonly ulong _tsmpGuildId = 952064632187658261;
    private SocketVoiceChannel _createChannel;

    public VCGroups(DiscordSocketClient client)
    {
        _client = client;
    }
    
    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;
        _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
    }

    private async Task ReadyAsync()
    {
        _createChannel = _client.GetGuild(_tsmpGuildId)
            .GetVoiceChannel(_createChannelId);
    }

    public async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
    {
       if (_createChannel is null) return;
       
        var category = _createChannel.Category;
       
        if (oldState.VoiceChannel is not null && oldState.VoiceChannel.CategoryId == category.Id && oldState.VoiceChannel.Id != _createChannel.Id)
        {
            Console.WriteLine($"debug stuff {oldState.VoiceChannel.Id} {oldState.VoiceChannel.Name} {oldState.VoiceChannel.ConnectedUsers.Count()}");
            var newChannel = oldState.VoiceChannel.Guild.GetVoiceChannel(oldState.VoiceChannel.Id);
            if (newChannel.ConnectedUsers.Count == 0)
            {
                await newChannel.DeleteAsync();
            }
        }
        else if (newState.VoiceChannel is not null && newState.VoiceChannel.CategoryId == category.Id && newState.VoiceChannel.Id == _createChannel.Id)
        {
            var newChannel = await category.Guild.CreateVoiceChannelAsync($"{user.Username}'s room", x => x.CategoryId = category.Id);
            var member = await category.Guild.GetUserAsync(user.Id);
            
            await member.ModifyAsync(x => x.Channel = Optional.Create(newChannel));
        }
        
    }
}