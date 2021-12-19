using BoTools.Service;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Run
{
    public class CommandHandler
    {
        private const ulong _vinceId = 312317884389130241;
        private const ulong _logChannelId = 826144013920501790;
        private const ulong _gamesMsgId = 848582017091108904;
        private const ulong _specialMsgId = 848582133994881054;
        private const ulong _readRuleMsgId = 848582652718219345;       
        
        private readonly DiscordSocketClient _client;
        private readonly RoleService _roleService;
        private readonly MessageService _messageService;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private static readonly char _commandPrefix = '$';        

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            _services = services;            

            _roleService ??= (RoleService)_services.GetService(typeof(RoleService));
            _messageService ??= (MessageService)_services.GetService(typeof(MessageService));
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
            _messageService.UserJoined(guildUser);
            await _roleService.UpdateListUser();
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            await AutoCheck(message);

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
            switch (arg1.Id)
            {
                case _gamesMsgId:
                    _roleService.GamesReactionRemoved(reaction);
                    break;
                case _specialMsgId:
                    _roleService.SpecialReactionRemoved(reaction);
                    break;
                case _readRuleMsgId:
                    _roleService.RulesReactionRemoved(reaction.UserId);
                    break;

                default: break;
            }
            return Task.CompletedTask;
        }

        //     The source channel of the reaction addition will be passed into the Discord.WebSocket.ISocketMessageChannel parameter.
        //     The reaction that was added will be passed into the Discord.WebSocket.SocketReaction parameter.
        private Task ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction reaction)
        {
            switch (arg1.Id)
            {
                case _gamesMsgId:
                    _roleService.GamesReactionAdded(reaction);
                    break;
                case _specialMsgId:
                    _roleService.SpecialReactionAdded(reaction);
                    break;
                case _readRuleMsgId:
                    _roleService.RulesReactionAdded(reaction.UserId);
                    break;

                //Si réaction by me dans log = OP dispo
                default:                   
                    if (reaction.User.Value.Id == _vinceId && arg2.Id == _logChannelId) 
                    {
                        _messageService.OnePieceDispo();
                    }
                    break;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Process that need a regular or systematic check on them
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private Task AutoCheck(SocketUserMessage message)
        {                     
            if (message.Content.ToLower().Contains("<@!825790068090339369>"))
                _messageService.AddReactionRobot(message);

            return Task.CompletedTask;
        }
    }
}
