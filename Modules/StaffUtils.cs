using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;

namespace Natsirt.Modules;

public class StaffUtils : InteractionModuleBase<SocketInteractionContext>
{
    private readonly NatsirtCave _cave;
    private readonly AdminAPI _client;
    private readonly IConfiguration _configuration;

    private readonly ulong _staffPingRoleId = 1054687518936793118;
    private readonly ulong _simpClubRoleId = 1058914301252993094;
    private InteractionHandler _handler;

    public StaffUtils(InteractionHandler handler, IConfiguration configuration, NatsirtCave cave, AdminAPI client)
    {
        _handler = handler;
        _configuration = configuration;
        _cave = cave;
        _client = client;
    }

    public InteractionService Commands { get; set; }

    [SlashCommand("toggle-staff", "toggle staff role")]
    public async Task ToggleStaffAsync()
    {
        await Context.Interaction.DeferAsync(true);

        var role = Context.Guild.GetRole(_staffPingRoleId);
        var member = Context.Guild.GetUser(Context.User.Id);

        if (member.Roles.Contains(role))
        {
            await member.RemoveRoleAsync(role);
            await Context.Interaction.FollowupAsync(
                "Enjoy your time off! Peers will no longer bug you for help and automation bots will no longer ping you.",
                ephemeral: true);
        }
        else
        {
            await member.AddRoleAsync(role);
            await Context.Interaction.FollowupAsync("Staff mode enabled!", ephemeral: true);
        }
    }
    
    [SlashCommand("toggle-simp", "toggle simp club role")]
    public async Task ToggleSimpAsync([Summary("target", "the user")]IGuildUser user, [Summary("public-shame", "whether or not to publicly shame the user")]bool publicShame = false)
    {
        await Context.Interaction.DeferAsync(!publicShame);

        var role = Context.Guild.GetRole(_simpClubRoleId);
        var member = Context.Guild.GetUser(user.Id);

        if (member.Roles.Contains(role))
        {
            await member.RemoveRoleAsync(role);
            await Context.Interaction.FollowupAsync(
                $"banished {user.Mention} from the tristan simp club...",
                ephemeral: !publicShame);
        }
        else
        {
            await member.AddRoleAsync(role);
            await Context.Interaction.FollowupAsync($"{user.Mention} has joined the tristan simp club 😭", ephemeral: !publicShame);
        }
    }
}