using DSharpPlus;

namespace FunBot.Logic.Discord
{
    public interface IDiscordBot
    {
        DiscordClient Client { get; }
        Task RunAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
