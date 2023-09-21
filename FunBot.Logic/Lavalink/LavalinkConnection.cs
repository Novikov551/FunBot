using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Net;
using FunBot.Logic.Configs;
using FunBot.Logic.Discord;
using Microsoft.Extensions.Options;

namespace FunBot.Logic.Lavalink
{
    public class LavalinkConnection : ILavalinkConnection
    {
        private readonly LavalinkConfig _lavalinkConfig;
        public LavalinkExtension Lavalink { get; set; }
        public LavalinkNodeConnection LavalinkNodeConnection { get; set; }
        public LavalinkGuildConnection LavalinkGuildConnection { get; set; }

        LavalinkExtension ILavalinkConnection.Lavalink { get; set; }

        LavalinkNodeConnection ILavalinkConnection.LavalinkNodeConnection { get; set; }

        LavalinkGuildConnection ILavalinkConnection.LavalinkGuildConnection { get; set; }

        private LavalinkConfiguration _lavalinkConfiguration;
        private ConnectionEndpoint _connectionEndpoint;

        public LavalinkConnection(IDiscordBot discrodBot,
            IOptions<LavalinkConfig> lavalinkConfig)
        {
            _lavalinkConfig = lavalinkConfig.Value;
            _connectionEndpoint = new ConnectionEndpoint
            {
                Hostname = _lavalinkConfig.Hostname, // From your server configuration.
                Port = _lavalinkConfig.Port, // From your server configuration
                Secured = true
            };

            _lavalinkConfiguration = new LavalinkConfiguration
            {
                Password = _lavalinkConfig.Password, // From your server configuration.
                RestEndpoint = _connectionEndpoint,
                SocketEndpoint = _connectionEndpoint
            };

            Lavalink = discrodBot.Client.UseLavalink();
        }

        public async Task ConnectAsync()
        {
            LavalinkNodeConnection = await Lavalink.ConnectAsync(_lavalinkConfiguration);
            LavalinkNodeConnection.Disconnected += ReconnectAsync;
        }

        private async Task ReconnectAsync(LavalinkNodeConnection sender, NodeDisconnectedEventArgs args)
        {
            LavalinkNodeConnection = await Lavalink.ConnectAsync(_lavalinkConfiguration);
        }

        public Task DisconnectAsync()
        {
            Lavalink.Dispose();

            return Task.CompletedTask;
        }
    }
}