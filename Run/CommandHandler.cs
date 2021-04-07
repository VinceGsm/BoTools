using BoTools.Service;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Run
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly JellyfinService _jellyfinService;
        private readonly MessageService _messageService;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private static readonly char _commandPrefix = '$';

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            _services = services;

            _jellyfinService ??= (JellyfinService)_services.GetService(typeof(JellyfinService));
        }

        public async Task InitializeCommandsAsync()
        {
            // Here we discover all of the command modules in the entry assembly and load them.
            // Starting from Discord.NET 2.0, a service provider is required to be passed into
            // the module registration method to inject the required dependencies
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);           

            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix(_commandPrefix, ref argPos) ||message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            await AutoCheck(message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);
        }

        /// <summary>
        /// Process that need a regular or systematic check on them
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task AutoCheck(SocketUserMessage message)
        {         
            await _jellyfinService.Clean();

            if (message.Content.ToLower().Contains("botools"))
                await _messageService.AddReactionRobot(message);

            return;
        }
    }
}
