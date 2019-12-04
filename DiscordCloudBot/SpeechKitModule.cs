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
            await using (var file = File.Create(guid + ".ogg"))
            {
                await using var audio = await _speechKitService.SpeakAsync(text);
                await audio.CopyToAsync(file);
                file.Close();
            }

            using var audioClient = await voiceChannel.ConnectAsync();
            await using var stream = audioClient.CreatePCMStream(AudioApplication.Voice);
            using var opus = RunOpusDecoder(guid);
            opus.WaitForExit();
            await using (var openWav = File.OpenRead($"{guid}.raw"))
            {
                await openWav.CopyToAsync(stream);
            }

            File.Delete($"{guid}.ogg");
            File.Delete($"{guid}.raw");
            await stream.FlushAsync();
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

        private Process RunOpusDecoder(Guid guid)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "opusdec",
                    Arguments =
                        $"--force-stereo {guid}.ogg {guid}.raw",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            return process;
        }
    }
}