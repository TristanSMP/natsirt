using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Natsirt;

public class TicketLogs : InteractionModuleBase<SocketInteractionContext>
{
    private readonly NatsirtCave _cave;
    private readonly AdminAPI _client;
    private readonly DiscordSocketClient _discord;
    private readonly IConfiguration _configuration;
    private readonly ulong _staffPingRoleId = 1054687518936793118;

    private readonly ulong _ticketCategoryId = 1072036891555266602;
    private readonly ulong _applicationsCategoryId = 1056793986817331241;
    private readonly ulong _ticketLogsChannelId = 1080766701601300490;
    private SocketForumChannel _ticketLogsChannel;

    public TicketLogs(IConfiguration configuration, NatsirtCave cave, AdminAPI client, DiscordSocketClient discord)
    {
        _configuration = configuration;
        _cave = cave;
        _client = client;
        _discord = discord;
    }
    
    public async Task InitializeAsync()
    {
        _discord.Ready += ReadyAsync;
        _discord.MessageReceived += MessageReceivedAsync;
        _discord.MessageUpdated += MessageUpdatedAsync;
    }
    
    private async Task ReadyAsync()
    {
        _ticketLogsChannel = _discord.GetChannel(_ticketLogsChannelId) as SocketForumChannel;
    }
    
    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (_ticketLogsChannel is not SocketForumChannel forum) return;
        if (message.Channel is not SocketTextChannel channel) return;
        if (channel.CategoryId != _ticketCategoryId && channel.CategoryId != _applicationsCategoryId) return;
        if (message.Author.IsBot) return;
        
        var embed = new EmbedBuilder()
            .WithColor(Color.Green)
            .WithAuthor(message.Author)
            .WithDescription(message.Content)
            .WithTimestamp(message.Timestamp)
            .Build();

        var post = await FindPostAsync(channel, forum);
        
        await post.SendMessageAsync(embed: embed);
    }
    
    private async Task MessageUpdatedAsync(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
    {
        if (_ticketLogsChannel is not SocketForumChannel forum) return;
        if (channel is not SocketTextChannel textChannel) return;
        if (textChannel.CategoryId != _ticketCategoryId && textChannel.CategoryId != _applicationsCategoryId) return;
        if (after.Author.IsBot) return;
        
        var embed = new EmbedBuilder()
            .WithColor(Color.Green)
            .WithAuthor(after.Author)
            .WithDescription($"`{before.Value.Content}`\n**->**\n`{after.Content}`")
            .WithTimestamp(after.Timestamp)
            .WithFooter("Edited")
            .Build();

        var post = await FindPostAsync(textChannel, forum);
        
        await post.SendMessageAsync(embed: embed);
    }

    private async Task<RestThreadChannel> FindPostAsync(SocketTextChannel forChannel, SocketForumChannel channel)
    {
        var posts = await channel.GetActiveThreadsAsync();
        var post = posts.FirstOrDefault(x => x.Name.Contains(forChannel.Id.ToString())) ??
                   await channel.CreatePostAsync($"<name> : {forChannel.Id}", text: $"**Ticket Log**\n**Members in ticket:** ```\n{string.Join("\n", forChannel.Users.Select(x => $"{x.Username}#{x.Discriminator} ({x.Id})"))}```\n*You can rename this thread to whatever you want, just keep the id in the name or things will break.*");

        return post;
    }
}