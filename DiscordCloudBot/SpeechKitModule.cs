using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;

namespace DiscordCloudBot
{
    public class SpeechKitModule : ModuleBase<SocketCommandContext>
    {
        private readonly SpeechKitService _speechKitService;

        public SpeechKitModule(SpeechKitService speechKitService)
        {
            _speechKitService = speechKitService;
        }

        [Command("say", RunMode = RunMode.Async)]
        public async Task SayAsync([Remainder] string text)
        {
            var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await Context.Channel.SendMessageAsync("The user must be in voice channel.");
                return;
            }

            var guid = Guid.NewGuid();
            var message = await Context.Channel.SendMessageAsync("Loading audio from Yandex Cloud...");
            await using (var file = File.Create(guid + ".raw"))
            {
                await using var audio = await _speechKitService.SpeakAsync(text);
                await audio.CopyToAsync(file);
                file.Close();
            }

            await message.ModifyAsync(x => x.Content = new Optional<string>("Sending audio to voice channel..."));

            using var audioClient = await voiceChannel.ConnectAsync();
            using var ffmpeg = RunFfmpegEncoder(guid);
            await using var output = ffmpeg.StandardOutput.BaseStream;
            await using var stream = audioClient.CreatePCMStream(AudioApplication.Voice);
            {
                try
                {
                    await output.CopyToAsync(stream);
                }
                finally
                {
                    await stream.FlushAsync();
                }
            }

            await message.ModifyAsync(x => x.Content = new Optional<string>("Deleting temp files..."));

            File.Delete($"{guid}.raw");

            await message.DeleteAsync();
            await Context.Message.DeleteAsync();
        }

        [Command("leave")]
        public async Task LeaveVoiceChannelAsync()
        {
            var voiceChannel = (Context?.User as IGuildUser)?.VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyAsync(":c");
                return;
            }

            await voiceChannel.DisconnectAsync();
            await Context.Message.DeleteAsync();
        }

        private Process RunFfmpegEncoder(Guid guid)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments =
                        $" -hide_banner -loglevel panic -f s16le -ar 48000 -ac 1 -i {guid}.raw -ac 2 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            return process;
        }
    }
}