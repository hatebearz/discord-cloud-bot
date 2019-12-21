using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordCloudBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            using var serviceProvider = BuildServiceProvider();
            var client = serviceProvider.GetService<DiscordSocketClient>();

            client.Log += LogAsync;
            serviceProvider.GetService<CommandService>().Log += LogAsync;


            var discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            if (string.IsNullOrEmpty(discordToken))
                throw new Exception("Environment variable DISCORD_TOKEN is not set.");
            await client.LoginAsync(TokenType.Bot, discordToken);
            await client.StartAsync();

            await serviceProvider.GetService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private ServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddTransient<SpeechKitService>()
                .BuildServiceProvider();
        }


        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}