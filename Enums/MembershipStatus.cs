using Discord.Interactions;

namespace Natsirt;

public enum MembershipStatus
{
    [ChoiceDisplay("Revoke")] NotMember,
    Member
}