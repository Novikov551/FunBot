using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FunBot.Logic.Discord;
using FunBot.Logic.Lavalink;

namespace FunBot.Logic.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServiceHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DBCONTEXT");//TO DO

            services.AddHealthChecks()
                .AddNpgSql(connectionString, name: "DBCONTEXT");
        }

        public static IServiceCollection AddLogic(this IServiceCollection services)
        {
            return services.AddSingleton<IDiscordBot, SDiscordBot>()
                    .AddSingleton<ILavalinkConnection, LavalinkConnection>()
                    .Scan(scan => scan.FromAssemblies(typeof(ServiceCollectionExtensions).Assembly)
                    .AddClasses(value => value.AssignableToAny())
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
        }
    }
}
