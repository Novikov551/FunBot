using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext;
using System.Text;
using DSharpPlus.Entities;

namespace FunBot.Logic.Discord.Commands
{
    public class HelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder _embed;
        protected StringBuilder _strBuilder;

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            _embed = new DiscordEmbedBuilder();
            _strBuilder = new StringBuilder();

            // Help formatters do support dependency injection.
            // Any required services can be specified by declaring constructor parameters. 

            // Other required initialization here ...
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _embed.Color = DiscordColor.Red;
            _embed.WithAuthor("Author: ?FOKACHCHA?", "", "https://steamuserimages-a.akamaihd.net/ugc/2020469857382547576/8EDD2C01D2AC7D9CB4D90AC8BA7ABFB491635A5C/");
            _embed.WithFooter("Старайтесь не злоупотреблять командами ;)");
            _embed.WithImageUrl("https://static.zerochan.net/Shenhe.full.3531685.jpg");
            _embed.WithTitle("Доступные команды сервера:");
            _embed.AddField(command.Name, command.Description);
            _strBuilder.AppendLine($"{command.Name} - {command.Description}");

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            foreach (var cmd in cmds)
            {
                _embed.Color = DiscordColor.Red;
                _embed.AddField(cmd.Name, cmd.Description);
                _embed.WithAuthor("Author: ?FOKACHCHA?", "", "https://steamuserimages-a.akamaihd.net/ugc/2020469857382547576/8EDD2C01D2AC7D9CB4D90AC8BA7ABFB491635A5C/");
                _embed.WithFooter("Старайтесь не злоупотреблять командами ;)");
                _embed.WithImageUrl("https://static.zerochan.net/Shenhe.full.3531685.jpg");
                _embed.WithTitle("Доступные команды сервера:");
                _strBuilder.AppendLine($"{cmd.Name} - {cmd.Description}");
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: _embed);
        }
    }
}
