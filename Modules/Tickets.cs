using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;

namespace Natsirt.Modules;

public class Tickets : InteractionModuleBase<SocketInteractionContext>
{
    private readonly NatsirtCave _cave;
    private readonly AdminAPI _client;
    private readonly IConfiguration _configuration;
    private readonly ulong _staffPingRoleId = 1054687518936793118;

    private readonly ulong _ticketCategoryId = 1072036891555266602;
    private InteractionHandler _handler;

    public Tickets(InteractionHandler handler, IConfiguration configuration, NatsirtCave cave, AdminAPI client)
    {
        _handler = handler;
        _configuration = configuration;
        _cave = cave;
        _client = client;
    }

    public InteractionService Commands { get; set; }

    [ComponentInteraction("tickets:new")]
    public async Task NewTicketAsync()
    {
        await Context.Interaction.DeferAsync(true);

        var existingTickets = Context.Guild.TextChannels
            .Where(x => x.Name.Contains(Context.User.Id.ToString()))
            .ToList();

        if (existingTickets.Count > 0)
        {
            await Context.Interaction.FollowupAsync($"You already have a ticket open! {existingTickets[0].Mention}",
                ephemeral: true);
            return;
        }

        var ticketChannel = await Context.Guild.CreateTextChannelAsync($"ticket-{Context.User.Id}", x =>
        {
            x.CategoryId = _ticketCategoryId;
            x.Topic = $"a support ticket for {Context.User.Username}!!!";
        });

        await ticketChannel.AddPermissionOverwriteAsync(Context.User,
            new OverwritePermissions(viewChannel: PermValue.Allow));


        var embed = new EmbedBuilder()
            .WithColor(Color.Purple)
            .WithDescription(
                "Welcome to the TSMP support hotline!\nPlease explain your issue below, and a staff member will be with you shortly.")
            .Build();


        await ticketChannel.SendMessageAsync($"{Context.User.Mention} <@&{_staffPingRoleId}>", embed: embed);

        await Context.Interaction.FollowupAsync($"Created a new ticket, {ticketChannel.Mention} 😙", ephemeral: true);
    }

    [SlashCommand("create-ticket-entrypoint", "tickets?")]
    public async Task CreateTicketEntrypointAsync(
        [Summary("a-string-please", "a string to please on to embed yes")] string desc)
    {
        await Context.Interaction.DeferAsync();

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithDescription(desc)
            .Build();

        var button = new ButtonBuilder()
            .WithLabel("Create Ticket")
            .WithStyle(ButtonStyle.Primary)
            .WithCustomId("tickets:new");

        var components = new ComponentBuilder()
            .WithButton(button)
            .Build();

        await Context.Interaction.FollowupAsync(embed: embed, components: components);
    }


    [SlashCommand("i-give-up", "give up being a moderator and use ai")]
    public async Task GiveUpAsync()
    {
        await Context.Interaction.RespondAsync(
            "not implemented, but this command will use ai to find out who in this ticket's side you should be on and what actions seem appropriate to take.",
            ephemeral: true);
    }

    [SlashCommand("add-to-ticket", "add a user to a ticket")]
    public async Task AddToTicketAsync(
        [Summary("user", "the user to add to the ticket")]
        IGuildUser user
    )
    {
        await Context.Interaction.DeferAsync();

        if (Context.Channel is not ITextChannel channel)
        {
            await Context.Interaction.FollowupAsync("this is not a ticket channel", ephemeral: true);
            return;
        }

        if (channel.CategoryId != _ticketCategoryId)
        {
            await Context.Interaction.FollowupAsync("this is not a ticket channel", ephemeral: true);
            return;
        }

        var ticketChannel = Context.Channel as ITextChannel;

        await ticketChannel.AddPermissionOverwriteAsync(user,
            new OverwritePermissions(viewChannel: PermValue.Allow));

        await Context.Interaction.FollowupAsync($"added {user.Mention} to the ticket", ephemeral: false);
    }

    [SlashCommand("remove-from-ticket", "remove a user from a ticket")]
    public async Task RemoveFromTicketAsync(
        [Summary("user", "the user to remove from the ticket")]
        IGuildUser user
    )
    {
        await Context.Interaction.DeferAsync();

        if (Context.Channel is not ITextChannel channel)
        {
            await Context.Interaction.FollowupAsync("this is not a ticket channel", ephemeral: true);
            return;
        }

        if (channel.CategoryId != _ticketCategoryId)
        {
            await Context.Interaction.FollowupAsync("this is not a ticket channel", ephemeral: true);
            return;
        }

        var ticketChannel = Context.Channel as ITextChannel;

        await ticketChannel.AddPermissionOverwriteAsync(user,
            new OverwritePermissions(viewChannel: PermValue.Deny));

        await Context.Interaction.FollowupAsync($"removed {user.Mention} from the ticket", ephemeral: false);
    }
}