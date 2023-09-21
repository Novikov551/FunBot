using FunBot.Logic.Configs;
using FunBot.Logic.Discord;
using FunBot.Logic.Lavalink;
using Microsoft.Extensions.Options;

namespace FunBot.Services
{
    public class StartService : IHostedService
    {
        private readonly ILogger<StartService> _logger;
        private readonly IServiceProvider _services;

        public StartService(ILogger<StartService> logger,
            IServiceProvider serviceProvider,
            IOptions<DiscordBotConfig> botOptions)
        {
            _logger = logger;
            _services = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<IDiscordBot>();
            var lavalinkConnection = scope.ServiceProvider.GetRequiredService<ILavalinkConnection>();

            _logger.LogInformation("Configuring the bot");

            _logger.LogInformation("Connect to the server");
            await botClient.RunAsync(cancellationToken);

            _logger.LogInformation("Connect to the lavalink");
            await lavalinkConnection.ConnectAsync();

            await Task.Delay(-1);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<IDiscordBot>();
            var lavalinkConnection = scope.ServiceProvider.GetRequiredService<ILavalinkConnection>();

            // App shutdown
            _logger.LogInformation("App shutdown");
            await lavalinkConnection.DisconnectAsync();
            await botClient.StopAsync(cancellationToken: cancellationToken);

            await Task.Delay(-1);
        }
    }
}