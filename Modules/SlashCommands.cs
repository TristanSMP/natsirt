using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Natsirt.Modules;

public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
{
    private InteractionHandler _handler;
    private readonly IConfiguration _configuration;
    private readonly NatsirtCave _cave;

    public SlashCommands(InteractionHandler handler, IConfiguration configuration, NatsirtCave cave)
    {
        _handler = handler;
        _configuration = configuration;
        _cave = cave;
    }
    
    public InteractionService Commands { get; set; }

    [SlashCommand("manage-membership", "manage membership of a player (what else???)")]
    public async Task ManageMembershipAsync(
        [Summary("player", "the player to manage")]
        IGuildUser player,
        [Summary("action", "the action to perform")]
        MembershipStatus action
    )
    {
        await Context.Interaction.DeferAsync();
        
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Authorization", _configuration["ADMIN_API_TOKEN"]);

        var data = new { player = player.Id.ToString(), action = action.ToString() };
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync($"{_configuration["ADMIN_API_ROOT"]}/application-manage", content);

        var embed = new EmbedBuilder();

        if (!response.IsSuccessStatusCode)
        {
            embed.WithColor(Color.Red)
                .WithDescription($"Failed to `{action.ToString().ToLower()}` {player.Mention}.");
            _cave.SendErrorToCave(await response.Content.ReadAsStringAsync());
        }
        else
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResult);

            embed.WithColor(Color.Green)
                .WithDescription(
                    $"Successfully {action.ToString().ToLower()}ed {player.Mention}.\n\n**Debug Info**\nMinecraft UUID: `{result["minecraftUUID"]}`\nMinecraft Username: `{result["minecraftUsername"]}`")
                .WithFields(new[]
                {
                    new EmbedFieldBuilder()
                        .WithName("Synced TSMP User")
                        .WithValue(result["syncedUser"])
                        .WithIsInline(true),
                    new EmbedFieldBuilder()
                        .WithName("Synced Discord Linked Role")
                        .WithValue(result["syncedRoleMeta"])
                        .WithIsInline(true),
                });

        }

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("refresh-all", "Refresh all users'.")]
    public async Task RefreshAll()
    {
        await Context.Interaction.DeferAsync();
        
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Authorization", _configuration["ADMIN_API_TOKEN"]);

        
        var response = await client.PostAsync($"{_configuration["ADMIN_API_ROOT"]}/refresh-everyone", null);

        var embed = new EmbedBuilder();

        if (!response.IsSuccessStatusCode) {
            embed.WithColor(Color.Red)
                .WithDescription($"Failed to refresh everyone.");
            _cave.SendErrorToCave(await response.Content.ReadAsStringAsync());
        }
        else
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonResult);

            embed.WithColor(Color.Green)
                .WithDescription(
                    $"Successfully refreshed everyone.\n\n**Debug Info**\nTotal Synced TSMP Users: `{result["totalSyncedTSMPUsers"]}`\nTotal Synced Discord Users: `{result["totalSyncedDiscordUsers"]}`\nTotal Failed Synced TSMP Users: `{result["totalFailedSyncedTSMPUsers"]}`\nTotal Failed Synced Discord Users: `{result["totalFailedSyncedDiscordUsers"]}`"
                );
        }
        
        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }
}