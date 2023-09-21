using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using FunBot.Logic.Discord.Commands.Attributes;
using FunBot.Logic.Exceptions.Lavalink;
using System;
using System.Xml.Linq;

namespace FunBot.Logic.Discord.Commands.Music
{
    [OnlyCommandChannel(1154048387830788187)]
    public class MusicCommands : ApplicationCommandModule
    {
        private List<LavalinkTrack> _musicTracks;

        private static (LavalinkExtension Lava, LavalinkNodeConnection NodeConnection) GetNodeConnection(InteractionContext context)
        {
            var lava = context.Client.GetLavalink();
            if (lava is null)
            {
                throw new LavalinkNotRegisteredException();
            }

            if (!lava.ConnectedNodes.Any())
            {
                throw new LavalinkConnectionNotEstablished();
            }

            var node = lava.ConnectedNodes.Values.FirstOrDefault();

            return (Lava: lava, NodeConnection: node);
        }

        private static async Task JoinAsync(InteractionContext ctx)
        {
            try
            {
                var (Lava, NodeConnection) = GetNodeConnection(ctx);
                var userVoiceChannel = ctx.Member.VoiceState.Channel;
                var checkUserVoice = await CheckUserVoiceStateAsync(ctx);
                var conn = await NodeConnection.ConnectAsync(ctx.Member.VoiceState.Channel);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Вход..."));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Не удалось войти в голосовой чат."));
            }
        }

        private static LavalinkGuildConnection GetGuildConnection(InteractionContext ctx, LavalinkNodeConnection node)
        {
            var conn = node.GetGuildConnection(ctx.Member.Guild);
            if (conn is null)
            {
                throw new NotConnectedToGuildException();
            }

            return conn;
        }

        public static async Task LeaveAsync(InteractionContext ctx)
        {
            try
            {
                var (Lava, NodeConnection) = GetNodeConnection(ctx);
                var userVoiceChannel = ctx.Member.VoiceState.Channel;
                var result = await CheckUserVoiceStateAsync(ctx);

                if (!result)
                {
                    return;
                }

                var conn = NodeConnection.GetGuildConnection(userVoiceChannel.Guild);

                await conn.DisconnectAsync();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Выход..."));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Не удалось покинуть голосовой канал"));
            }
        }

        private static async Task<bool> CheckUserVoiceStateAsync(InteractionContext ctx)
        {
            var userVoiceChannel = ctx.Member.VoiceState.Channel;

            if (userVoiceChannel is null || userVoiceChannel.Type != ChannelType.Voice)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Вы не в голосовом канале"));

                return false;
            }

            return true;
        }

        private static async Task<IReadOnlyCollection<LavalinkTrack>> LoadLavalinkTracksAsync(LavalinkNodeConnection node,
            string search,
            LavalinkSearchType lavalinkSearch)
        {
            var loadResult = await node.Rest.GetTracksAsync(search, lavalinkSearch);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                throw new FailedToLoadLavalinkTracksException();
            }

            return loadResult.Tracks.Take(5).ToList();
        }

        private static async Task<LavalinkTrack> LoadLavalinkTracksAsync(LavalinkNodeConnection nodeConnection, Uri uri)
        {
            var loadResult = await nodeConnection.Rest.GetTracksAsync(uri);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                throw new FailedToLoadLavalinkTracksException();
            }

            return loadResult.Tracks.First();
        }

        [Category("Music")]
        [SlashCommand("play", "Поиск треков и их воспроизведение")]
        public async Task Play(InteractionContext ctx,
            [Option("Название:", "Название трека")][RemainingText] string search,
            [Option("Источник:", "Выбор источника воспроизведения")] LavalinkSearchType lavalinkSearch = LavalinkSearchType.Youtube)
        {
            try
            {
                ctx.Client.ComponentInteractionCreated += SelectMusicHandler;

                var (Lava, NodeConnection) = GetNodeConnection(ctx);
                var checkResult = await CheckUserVoiceStateAsync(ctx);
                var tracks = await LoadLavalinkTracksAsync(NodeConnection,
                    search,
                    lavalinkSearch);

                var contentStr = "Выберите трек из списка:\n";

                var trackButtons = new List<DiscordComponent>();
                var counter = 1;

                foreach (var track in tracks)
                {
                    contentStr += $"{counter} \"{track.Title} {track.Length}\"\n";

                    trackButtons.Add(new DiscordButtonComponent(ButtonStyle.Primary,
                        track.Identifier,
                        counter.ToString()));

                    counter++;
                }

                var message = await ctx.Client.SendMessageAsync(ctx.Channel,
                    new DiscordMessageBuilder()
                    .WithContent(contentStr)
                    .AddComponents(trackButtons));

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Поиск треков..."));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Не удалось выполнить поиск."));
            }
        }

        private async Task SelectMusicHandler(DiscordClient sender,
            ComponentInteractionCreateEventArgs args)
        {
            try
            {
                sender.ComponentInteractionCreated -= SelectMusicHandler;

                var lava = sender.GetLavalink();
                if (lava is null)
                {
                    throw new LavalinkNotRegisteredException();
                }

                if (!lava.ConnectedNodes.Any())
                {
                    throw new LavalinkConnectionNotEstablished();
                }

                var node = lava.ConnectedNodes.Values.FirstOrDefault();
                if (node is null)
                {
                    throw new LavalinkConnectionNotEstablished();
                }

                var user = args.User;
                var guild = args.Guild;
                var userVoiceChannel = guild.Channels.FirstOrDefault(c => c.Value.Type == ChannelType.Voice && c.Value.Users.Contains(user));
                if (userVoiceChannel.Value is null)
                {
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .WithContent("Вы не в голосовом канале"));

                    return;
                }

                var conn = node.GetGuildConnection(guild);
                if (conn is null || !conn.IsConnected)
                {
                    throw new NotConnectedToGuildException();
                }

                if (_musicTracks is null || !_musicTracks.Any())
                {
                    throw new ArgumentOutOfRangeException(nameof(_musicTracks));
                }

                var track = _musicTracks.First(t => t.Identifier == args.Interaction.Data.CustomId);

                await conn.PlayAsync(track);

                _musicTracks = null;

                await sender.SendMessageAsync(args.Channel,
                    new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = $"Сейчас играет: {track.Title}\nAuthor: {track.Author}\nUrl:{track.Uri}"
                    });

                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Запуск..."));
            }
            catch (Exception ex)
            {
                await sender.SendMessageAsync(args.Channel,
                    new DiscordMessageBuilder()
                    .WithContent("Не удалось воспроизвести указанный трек, возможно он был удален, попробуйте другой."));
            }
        }


        [Category("Music")]
        [SlashCommand("playByUrl", "Поиск треков по ссылке и их воспроизведение")]
        public async Task Play(InteractionContext ctx, [Option("адрес:", "Ссылка.")] string url)
        {
            try
            {
                var (Lava, NodeConnection) = GetNodeConnection(ctx);
                var checkResult = await CheckUserVoiceStateAsync(ctx);
                if (!checkResult)
                {
                    return;
                }

                var conn = GetGuildConnection(ctx, NodeConnection);
                var track = await LoadLavalinkTracksAsync(NodeConnection, new Uri(url));

                await conn.PlayAsync(track);

                await ctx.Client.SendMessageAsync(ctx.Channel,
                    new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = $"Сейчас играет: {track.Title}\nAuthor: {track.Author}\nUrl:{track.Uri}"
                    });

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Поиск..."));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Не удалось воспроизвести, попробуйте позже..."));
            }
        }



        [Category("Music")]
        [SlashCommand("pause", "Приостановление проигрывателя")]
        public async Task Pause(InteractionContext ctx)
        {
            try
            {
                var (Lava, NodeConnection) = GetNodeConnection(ctx);
                var checkResult = await CheckUserVoiceStateAsync(ctx);
                if (!checkResult)
                {
                    return;
                }

                var conn = GetGuildConnection(ctx, NodeConnection);

                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.Client.SendMessageAsync(ctx.Channel,
                        new DiscordMessageBuilder()
                        .WithContent("В данный момент не воспроизводится трек"));

                    return;
                }

                await conn.PauseAsync();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Пауза"));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("Не удалось приостановить вопсроизведение"));
            }
        }

        [Category("Music")]
        [SlashCommand("resume", "Продолжение")]
        public async Task Resume(InteractionContext ctx)
        {
            try
            {
                var (Lava, NodeConnection) = GetNodeConnection(ctx);
                var checkResult = await CheckUserVoiceStateAsync(ctx);
                if (!checkResult)
                {
                    return;
                }

                var conn = GetGuildConnection(ctx, NodeConnection);

                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.Client.SendMessageAsync(ctx.Channel,
                        new DiscordMessageBuilder()
                        .WithContent("Отсутствует трек"));

                    return;
                }

                await conn.ResumeAsync();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Запуск"));
            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Не удалось продолжить вопсроизведение"));
            }
        }

        [Category("Volume")]
        [SlashCommand("volume", "Приостановление проигрывателя")]
        public async Task Volume(InteractionContext ctx, [Option("value", "volume value")] string volume)
        {
            try
            {
                var (Lava, NodeConnection) = GetNodeConnection(ctx);
                var checkResult = await CheckUserVoiceStateAsync(ctx);
                if (!checkResult)
                {
                    return;
                }

                var conn = GetGuildConnection(ctx, NodeConnection);

                if (conn.CurrentState.CurrentTrack == null)
                {
                    await ctx.Client.SendMessageAsync(ctx.Channel,
                        new DiscordMessageBuilder()
                        .WithContent("Отсутствует трек"));

                    return;
                }

                if (int.TryParse(volume, out var value))
                {

                    await conn.SetVolumeAsync(value);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .WithContent("Настройка громкости"));
                }
                else
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .WithContent("Не удалось выставить громкость."));
                }

            }
            catch (Exception ex)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent("Не удалось выставить громкость."));
            }
        }
    }
}