using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordCloudBot
{
    public class Program
    {
        private DiscordSocketClient _client;

        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            await _client.LoginAsync(TokenType.Bot,
                Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await _client.StartAsync();
            await Task.Delay(-1);
        }


        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}