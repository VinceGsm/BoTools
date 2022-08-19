using BoTools.Service;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Run
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interaction;
        private readonly IServiceProvider _services;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public InteractionHandler(DiscordSocketClient client, InteractionService interaction, IServiceProvider services)
        {
            _interaction = interaction;
            _client = client;
            _services = services;            
        }

        public async Task InitializeInteractionAsync()
        {
            // Here we discover all of the command modules in the entry assembly and load them.
            // Starting from Discord.NET 2.0, a service provider is required to be passed into
            // the module registration method to inject the required dependencies
            await _interaction.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
            
            _client.InteractionCreated += HandleInteractionCreated;            
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
    }
}
