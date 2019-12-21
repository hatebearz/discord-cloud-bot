using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordCloudBot
{
    public class TranslateModule : ModuleBase<SocketCommandContext>
    {
        private readonly TranslateService _translateService;

        public TranslateModule(TranslateService translateService)
        {
            _translateService = translateService;
        }

        [Command("переведи", RunMode = RunMode.Async)]
        public async Task TranslateAsync([Remainder] string text)
        {
            var translatedText = await _translateService.TranslateToRussianAsync(text);

            await Context.Channel.SendMessageAsync(translatedText);
        }
    }
}