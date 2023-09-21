using DSharpPlus.Lavalink;

namespace FunBot.Logic.Lavalink
{
    public interface ILavalinkConnection
    {
        LavalinkExtension Lavalink { get; set; }
        LavalinkNodeConnection LavalinkNodeConnection { get; set; }
        LavalinkGuildConnection LavalinkGuildConnection { get; set; }

        Task ConnectAsync();
        Task DisconnectAsync();
    }
}