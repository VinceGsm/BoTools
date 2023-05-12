using BoTools.Service;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using log4net;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BoTools.Run
{
    public class InteractionHandler
    {
        private const ulong _readRuleMsgId = 848582652718219345;

        private readonly RoleService _roleService;        
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly IServiceProvider _services;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public InteractionHandler(DiscordSocketClient client, InteractionService interaction, IServiceProvider services)
        {
            _interaction = interaction;
            _client = client;
            _services = services;

            _roleService ??= (RoleService)_services.GetService(typeof(RoleService));
        }

        public async Task InitializeInteractionAsync()
        {
            // Here we discover all of the command modules in the entry assembly and load them.
            // Starting from Discord.NET 2.0, a service provider is required to be passed into
            // the module registration method to inject the required dependencies
            await _interaction.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
            
            _client.InteractionCreated += HandleInteractionCreated;
            _client.UserJoined += UserJoined;
            _client.ReactionAdded += ReactionAdded;
            _client.ReactionRemoved += ReactionRemoved;
        }

        private async Task HandleInteractionCreated(SocketInteraction arg)
        {
            try
            {
                var context = new SocketInteractionContext(_client, arg);
                await _interaction.ExecuteCommandAsync(context, _services);
            }
            catch(Exception ex) { log.Error(ex.ToString()); }
        }

        private async Task UserJoined(SocketGuildUser guildUser)
        {
            await _roleService.UpdateListUser();
        }

        private Task ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction reaction)
        {
            if (arg1.Id == _readRuleMsgId)
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
