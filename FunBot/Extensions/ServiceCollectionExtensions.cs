using FunBot.Logic.Configs;

namespace FunBot.Extensions
{
    public static class ConfigExtensions
    {
        public static void AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DiscordBotConfig>(configuration.GetSection(nameof(DiscordBotConfig)));
            services.Configure<LavalinkConfig>(configuration.GetSection(nameof(LavalinkConfig)));
        }
    }
}
