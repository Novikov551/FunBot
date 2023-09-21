using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using FunBot.Logic.Discord.Commands.Attributes;

namespace FunBot.Logic.Discord.Commands.Music
{
    [OnlyCommandChannel(1154048387830788187)]
    public class MusicCommands : ApplicationCommandModule
    {
        private List<LavalinkTrack> _musicTracks;

        [SlashCommand("join", "Join the bot in user voice channel")]
        public async Task Join(InteractionContext ctx, [Option("Channel", "Голосовой канал:")] DiscordChannel channel)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent("The Lavalink connection is not established"));

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Join..."));

                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent("Not a valid voice channel."));

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Join..."));

                return;
            }

            await node.ConnectAsync(channel);

            await ctx.Client.SendMessageAsync(ctx.Channel,
                    new DiscordMessageBuilder()
                    .WithContent($"Joined {channel.Name}!"));

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Join..."));

        }

        [SlashCommand("leave", "Leave the bot in user voice channel")]
        public async Task Leave(InteractionContext ctx, [Option("Channel", "Голосовой канал:")] DiscordChannel channel)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                    new DiscordMessageBuilder()
                    .WithContent("The Lavalink connection is not established"));

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Leave..."));

                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                  new DiscordMessageBuilder()
                  .WithContent("Not a valid voice channel."));

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                   new DiscordInteractionResponseBuilder()
                  .WithContent("Leave..."));

                return;
            }


            var conn = node.GetGuildConnection(ctx.Member.Guild);

            if (conn == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                  new DiscordMessageBuilder()
                  .WithContent("Lavalink is not connected."));

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Leave..."));

                return;
            }

            await conn.DisconnectAsync();

            await ctx.Client.SendMessageAsync(ctx.Channel,
                    new DiscordMessageBuilder()
                  .WithContent($"Left {channel.Name}!"));

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
               .WithContent("Leave channel..."));
        }

        [Category("Music")]
        [SlashCommand("play", "Поиск треков и их воспроизведение")]
        public async Task Play(InteractionContext ctx,
            [Option("Search", "Название трека:")][RemainingText] string search,
            [Option("SearchType", "Выбор источника воспроизведения:")] LavalinkSearchType lavalinkSearch = LavalinkSearchType.Youtube)
        {
            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Searching tracks..."));

                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent("You are not in a voice channel."));

                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (node == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Searching tracks..."));

                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent("Lavalink is not connected."));

                return;
            }

            //We don't need to specify the search type here
            //since it is YouTube by default.

            var loadResult = await node.Rest.GetTracksAsync(search, lavalinkSearch);

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Searching tracks..."));

                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent($"Track search failed for {search}."));

                return;
            }

            _musicTracks = new();

            var tracks = loadResult.Tracks.Take(5);

            _musicTracks.AddRange(tracks);

            var contentStr = "Выберите трек из списка:\n";

            var trackButtons = new List<DiscordComponent>();
            var counter = 1;

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
               .WithContent("Searching tracks..."));

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

            ctx.Client.ComponentInteractionCreated += SelectMusicHandler;
        }

        private async Task SelectMusicHandler(DiscordClient sender,
            ComponentInteractionCreateEventArgs args)
        {
            var user = args.User;
            var userChannel = args.Guild.Channels.FirstOrDefault(c => c.Value.Type == ChannelType.Voice 
            && c.Value.Users.FirstOrDefault(u=>u.Id == user.Id) is not null);

            if (_musicTracks is null || !_musicTracks.Any())
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Start playing..."));

                await sender.SendMessageAsync(args.Channel,
                    new DiscordMessageBuilder()
                    .WithContent("Track not founded."));
            }

            var lava = sender.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();

            if (node == null)
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Start playing..."));

                await sender.SendMessageAsync(args.Channel,
                    new DiscordMessageBuilder()
                    .WithContent("Lavalink is not connected."));

                return;
            }

            await node.ConnectAsync(userChannel.Value);

            var track = _musicTracks.FirstOrDefault(t => t.Identifier == args.Interaction.Data.CustomId);
            if (track == null)
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                   .WithContent("Start playing..."));

                await sender.SendMessageAsync(args.Channel,
                    new DiscordMessageBuilder()
                    .WithContent("Не удалось воспроизвести указанный трек, возможно он был удален, попробуйте другой."));

                return;
            }

            var conn = node.GetGuildConnection(userChannel.Value.Guild);
            await conn.PlayAsync(track);
            
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
               .WithContent("Start playing..."));

            await sender.SendMessageAsync(args.Channel,
                new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = $"Now playing: {track.Title}\nAuthor: {track.Author}\nUrl:{track.Uri}"
                });
        }

        
        [Category("Music")]
        [SlashCommand("playByUrl", "Поиск треков по ссылке и их воспроизведение")]
        public async Task Play(InteractionContext ctx, [Option("Uri", "Ссылка:")] string url)
        {
            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                  new DiscordInteractionResponseBuilder()
                 .WithContent("Searching track..."));

                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent("You are not in a voice channel."));

                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                  new DiscordInteractionResponseBuilder()
                 .WithContent("Searching track..."));

                await ctx.Client.SendMessageAsync(ctx.Channel,
                  new DiscordMessageBuilder()
                  .WithContent("Lavalink is not connected."));

                return;
            }

            //We don't need to specify the search type here
            //since it is YouTube by default.
            var loadResult = await node.Rest.GetTracksAsync(new Uri(url));

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed
                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                  new DiscordInteractionResponseBuilder()
                 .WithContent("Searching track..."));

                await ctx.Client.SendMessageAsync(ctx.Channel,
                  new DiscordMessageBuilder()
                  .WithContent($"Track search failed for {url}."));

                return;
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await ctx.Client.SendMessageAsync(ctx.Channel,
                  new DiscordMessageBuilder()
                  .WithContent($"Now playing {track.Title}!"));

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                 new DiscordInteractionResponseBuilder()
                 .WithContent("Searching track..."));
        }

        [Category("Music")]
        [SlashCommand("pause", "Приостановление проигрывателя")]
        public async Task Pause(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
              .WithContent("Pause track..."));

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent("You are not in a voice channel."));

                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                  .WithContent("Lavalink is not connected."));

                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                  new DiscordMessageBuilder()
                  .WithContent("There are no tracks loaded."));

                return;
            }

            await conn.PauseAsync();
        }

        [Category("Music")]
        [SlashCommand("resume", "Продолжение")]
        public async Task Resume(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
              .WithContent("Resume track..."));

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent("You are not in a voice channel."));

                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                  .WithContent("Lavalink is not connected."));

                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                  new DiscordMessageBuilder()
                  .WithContent("There are no tracks loaded."));

                return;
            }

            await conn.ResumeAsync();
        }

        [Category("Volume")]
        [SlashCommand("volume", "Приостановление проигрывателя")]
        public async Task Volume(InteractionContext ctx, [Option("value", "volume value")] string volume)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
              .WithContent("Set volume..."));

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                   .WithContent("You are not in a voice channel."));

                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                  .WithContent("You not in voice channel."));

                return;
            }

            await ctx.Client.SendMessageAsync(ctx.Channel,
                   new DiscordMessageBuilder()
                  .WithContent($"Volume now: {volume}"));

            var value = int.Parse(volume);

            await conn.SetVolumeAsync(value);
        }


    }
}