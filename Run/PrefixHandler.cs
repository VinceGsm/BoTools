using BoTools.Service;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Run
{
    public class PrefixHandler
    {        
        private const ulong _readRuleMsgId = 848582652718219345;       
        
        private readonly DiscordSocketClient _client;
        private readonly RoleService _roleService;        
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private static readonly char _commandPrefix = '$';        

        public PrefixHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            _services = services;            

            _roleService ??= (RoleService)_services.GetService(typeof(RoleService));            
        }

        public async Task InitializeCommandsAsync()
        {
            // Here we discover all of the command modules in the entry assembly and load them.
            // Starting from Discord.NET 2.0, a service provider is required to be passed into
            // the module registration method to inject the required dependencies
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
            
            _client.UserJoined += UserJoined;
            _client.ReactionAdded += ReactionAdded;            
            _client.ReactionRemoved += ReactionRemoved;
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task UserJoined(SocketGuildUser guildUser)
        {                        
            await _roleService.UpdateListUser();
        }

        public void AddModule<T>()
        {
            _commands.AddModuleAsync<T>(null);
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

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(context: context, argPos: argPos, services: _services);
        }

        private Task ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction reaction)
        {
            if(arg1.Id == _readRuleMsgId)
                _roleService.RulesReactionRemoved(reaction.UserId);

            return Task.CompletedTask;
        }

        //     The source channel of the reaction addition will be passed into the Discord.WebSocket.ISocketMessageChannel parameter.
        //     The reaction that was added will be passed into the Discord.WebSocket.SocketReaction parameter.
        private Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction reaction)
        {
            if (arg1.Id == _readRuleMsgId)
                _roleService.RulesReactionRemoved(reaction.UserId);

            return Task.CompletedTask;
        }
    }
}