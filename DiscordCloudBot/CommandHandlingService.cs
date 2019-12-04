using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordCloudBot
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandlingService(
            DiscordSocketClient client, 
            CommandService commands,
            IServiceProvider serviceProvider)
        {
            _client = client;
            _commands = commands;
            _serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage)) return;
            var argPos = 0;
            if (!(userMessage.HasCharPrefix('!', ref argPos) ||
                  userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                userMessage.Author.IsBot)
                return;
            var context = new SocketCommandContext(_client, userMessage);
            var result = await _commands.ExecuteAsync(context, argPos, _serviceProvider);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}