using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Natsirt.Modules;

public class AdminApi : InteractionModuleBase<SocketInteractionContext>
{
    private readonly NatsirtCave _cave;
    private readonly AdminAPI _client;
    private readonly IConfiguration _configuration;
    private InteractionHandler _handler;

    public AdminApi(InteractionHandler handler, IConfiguration configuration, NatsirtCave cave, AdminAPI client)
    {
        _handler = handler;
        _configuration = configuration;
        _cave = cave;
        _client = client;
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

        var data = new { player = player.Id.ToString(), action = action.ToString() };
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync($"{_configuration["ADMIN_API_ROOT"]}/application-manage", content);

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
                .WithFields(new EmbedFieldBuilder()
                    .WithName("Synced TSMP User")
                    .WithValue(result["syncedUser"])
                    .WithIsInline(true), new EmbedFieldBuilder()
                    .WithName("Synced Discord Linked Role")
                    .WithValue(result["syncedRoleMeta"])
                    .WithIsInline(true));
        }

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("refresh-all", "Refresh all users'.")]
    public async Task RefreshAll()
    {
        await Context.Interaction.DeferAsync();

        var response = await _client.PostAsync($"{_configuration["ADMIN_API_ROOT"]}/refresh-everyone", null);

        var embed = new EmbedBuilder();

        if (!response.IsSuccessStatusCode)
        {
            embed.WithColor(Color.Red)
                .WithDescription("Failed to refresh everyone.");
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

    [SlashCommand("ign-lookup", "in-game name -(moyang)> uuid -(tsmp)> discord")]
    public async Task IGNLookupAsync(
        [Summary("ign", "the in-game name to lookup")]
        string ign
    )
    {
        await Context.Interaction.DeferAsync();

        var response = await _client.GetAsync($"{_configuration["ADMIN_API_ROOT"]}/ign-lookup?ign={ign}");

        var embed = new EmbedBuilder();

        if (!response.IsSuccessStatusCode)
        {
            embed.WithColor(Color.Red)
                .WithDescription($"Failed to lookup `{ign}`.");
            _cave.SendErrorToCave(await response.Content.ReadAsStringAsync());
        }
        else
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResult);

            embed.WithColor(Color.Green)
                .WithDescription(
                    $"Successfully looked up `{ign}`.\n\n**Debug Info**\nMinecraft UUID: `{result["minecraftUUID"]}`\nMinecraft Username: `{result["minecraftUsername"]}`\nDiscord: <@{result["discordId"]}> (`{result["discordId"]}`)"
                );
        }

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }


    [SlashCommand("alts", "Get a user's alts.")]
    public async Task GetAltsAsync(
        [Summary("user", "the user to get alts for")]
        IGuildUser user
    )
    {
        await Context.Interaction.DeferAsync();

        var response = await _client.GetAsync($"{_configuration["ADMIN_API_ROOT"]}/alts?discordId={user.Id}");

        var embed = new EmbedBuilder();

        if (!response.IsSuccessStatusCode)
        {
            embed.WithColor(Color.Red)
                .WithDescription($"Failed to get alts for {user.Mention}.");
            _cave.SendErrorToCave(await response.Content.ReadAsStringAsync());
        }
        else
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonResult);

            embed.WithColor(Color.Green)
                .WithDescription(
                    $"Successfully got alts for {user.Mention}.\n\n**Debug Info**\nAlts: `{string.Join(", ", result["alts"])}`"
                );
        }

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("add-alt", "Add an alt to a user.")]
    public async Task AddAltAsync(
        [Summary("user", "the user to add an alt to")]
        IGuildUser user,
        [Summary("alt", "the alt to add")] string alt
    )
    {
        await Context.Interaction.DeferAsync();
        var data = new { discordId = user.Id.ToString(), alt };
        var response = await _client.PostAsync($"{_configuration["ADMIN_API_ROOT"]}/alts",
            new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));

        var embed = new EmbedBuilder();

        if (!response.IsSuccessStatusCode)
        {
            embed.WithColor(Color.Red)
                .WithDescription($"Failed to add alt `{alt}` to {user.Mention}.");
            _cave.SendErrorToCave(await response.Content.ReadAsStringAsync());
        }
        else
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonResult);

            embed.WithColor(Color.Green)
                .WithDescription(
                    $"Successfully added alt `{alt}` to {user.Mention}.\n\n**Debug Info**\nAlts: `{string.Join(", ", result["alts"])}`"
                );
        }

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("remove-alt", "Remove an alt from a user.")]
    public async Task RemoveAltAsync(
        [Summary("user", "the user to remove an alt from")]
        IGuildUser user,
        [Summary("alt", "the alt to remove")] string alt
    )
    {
        await Context.Interaction.DeferAsync();

        var data = new Dictionary<string, string>
        {
            { "discordId", user.Id.ToString() },
            { "alt", alt }
        };

        var response = await _client.PostAsync($"{_configuration["ADMIN_API_ROOT"]}/alts",
            new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));

        var embed = new EmbedBuilder();

        if (!response.IsSuccessStatusCode)
        {
            embed.WithColor(Color.Red)
                .WithDescription($"Failed to remove alt `{alt}` from {user.Mention}.");
            _cave.SendErrorToCave(await response.Content.ReadAsStringAsync());
        }
        else
        {
            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(jsonResult);

            embed.WithColor(Color.Green)
                .WithDescription(
                    $"Successfully removed alt `{alt}` from {user.Mention}.\n\n**Debug Info**\nAlts: `{string.Join(", ", result["alts"])}`"
                );
        }

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }
}