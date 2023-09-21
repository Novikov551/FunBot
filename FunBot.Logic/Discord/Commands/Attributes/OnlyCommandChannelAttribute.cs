using DSharpPlus.SlashCommands;

namespace FunBot.Logic.Discord.Commands.Attributes
{
    public class OnlyCommandChannelAttribute : SlashCheckBaseAttribute
    {
        public ulong ChannelId;

        public OnlyCommandChannelAttribute(ulong channelId)
        {
            this.ChannelId = channelId;
        }

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (ctx.Channel.Id == ChannelId)
                return true;
            else
                return false;
        }
    }
}
