using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using FunBot.Logic.Configs;
using FunBot.Logic.Discord.Commands;
using FunBot.Logic.Discord.Commands.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System.Reflection;

namespace FunBot.Logic.Discord
{
    public class SDiscordBot : IDiscordBot
    {
        public DiscordClient Client { get; set; }
        protected Serilog.ILogger _logger;
        protected DiscordBotConfig _discordConfig;

        public SDiscordBot(IOptions<DiscordBotConfig> discordConfig,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _discordConfig = discordConfig.Value;

            ConfigureLogging(configuration);
            ConfigureClient();
            ConfigureCommands(serviceProvider);
        }

        private void ConfigureLogging(IConfiguration configuration)
        {
            _logger = new LoggerConfiguration()
              .ReadFrom.Configuration(configuration)
              .CreateLogger();
        }

        private void ConfigureClient()
        {
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = _discordConfig.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                LoggerFactory = new LoggerFactory().AddSerilog(_logger),
                AutoReconnect = true,

            });

            Client.MessageCreated += MessageCreatedHandler;
            Client.GuildMemberAdded += MemberAddedHandler;
        }



        private void ConfigureCommands(IServiceProvider services)
        {
            var slash = Client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });
            slash.RegisterCommands(Assembly.GetExecutingAssembly(), 908784480079188008);
            slash.SlashCommandErrored += SlshErroredHandler;

            var commands = Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });

            commands.CommandErrored += CmdErroredHandler;
            commands.SetHelpFormatter<HelpFormatter>();
            commands.RegisterConverter(new ArgumentConverter());
            commands.RegisterCommands(Assembly.GetExecutingAssembly());

        }

        private async Task SlshErroredHandler(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
        {
            if (args.Exception is SlashExecutionChecksFailedException slex)
            {
                foreach (var check in slex.FailedChecks)
                    if (check is OnlyCommandChannelAttribute att)
                        await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Only <@{att.ChannelId}> can run this command!"));
            }
        }

        private async Task CmdErroredHandler(CommandsNextExtension _, CommandErrorEventArgs e)
        {
            var failedChecks = ((ChecksFailedException)e.Exception).FailedChecks;
            foreach (var failedCheck in failedChecks)
            {

            }
        }

        private Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e)
        {
            return Task.CompletedTask;
        }

        private Task MemberAddedHandler(DiscordClient s, GuildMemberAddEventArgs e)
        {
            return Task.CompletedTask;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await Client.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await Client.DisconnectAsync();
        }
    }
}
