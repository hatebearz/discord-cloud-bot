using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordCloudBot
{
    internal class Program : IDisposable
    {
        private readonly StreamWriter _logWriter;

        public void Dispose()
        {
            _logWriter.Dispose();
        }

        private Program()
        {
            _logWriter = File.CreateText("log.txt");
            _logWriter.AutoFlush = true;
        }

        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var program = new Program();
            try
            {
                program.MainAsync().GetAwaiter().GetResult();
            }
            catch (Exception exc)
            {
                program.LogAsync(exc).GetAwaiter().GetResult();
                program.Dispose();
            }
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
                .AddSingleton<TranslateService>()
                .BuildServiceProvider();
        }


        private async Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            await _logWriter.WriteLineAsync(message.ToString());
        }

        private async Task LogAsync(Exception exception)
        {
            await _logWriter.WriteLineAsync($"{DateTime.UtcNow}: Unhandled exception occured.");
            await _logWriter.WriteLineAsync($"Message: {exception.Message}");
        }
    }
}