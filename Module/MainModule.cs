using BoTools.Service;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Module
{
    // Keep in mind your module must be public and inherit ModuleBase to be discovered by AddModulesAsync.    
    public class MainModule : ModuleBase<SocketCommandContext>
    {
        private bool _isRunning = false;
        private readonly MessageService _messageService;
        private readonly RoleService _roleService;
        private readonly JellyfinService _jellyfinService;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);        


		public MainModule(MessageService messageService, JellyfinService jellyfinService, RoleService roleService) 
		{
			_jellyfinService = jellyfinService;
			_messageService = messageService;
            _roleService = roleService;
        }


        #region Jellyfin
        [Command("Jellyfin")]
        [Summary("Active et partage un lien secret d'accès au Jellyfin privé de Vince")]
        public async Task JellyfinAsync()
        {
            string message = string.Empty;
            SocketUserMessage userMsg = Context.Message;
            log.Info($"JellyfinAsync by {userMsg.Author}");

            var reference = new MessageReference(userMsg.Id);
            if (Helper.IsJellyfinCorrectChannel(Context.Channel))
            {
                if (!_isRunning)
                {
                    await _jellyfinService.ClearChannel(Context.Client);                    
                    await _messageService.AddReactionVu(userMsg);

                    //activation Jellyfin
                    _jellyfinService.Activate();
                    log.Info($"Jellyfin activated");

                    //activation NGrock + récupération du lien http
                    string ngrokUrl = await _jellyfinService.GetNgrokUrl();
                    log.Info($"ngrokUrl = {ngrokUrl}");

                    var builder = _messageService.MakeJellyfinMessageBuilder(userMsg, ngrokUrl);
                    Embed embed = builder.Build();

                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday) //
                        message = $"{_messageService.GetLuffyEmote()}";
                    else
                        message = $"{_messageService.GetPepeSmokeEmote()}";

                    await Context.Channel.SendMessageAsync(message, false, embed, null, null, reference);
                    await _messageService.AddDoneReaction(userMsg);
                    _isRunning = true;
                }
                else
                {
                    await _messageService.AddReactionRefused(userMsg);
                    await _messageService.SendJellyfinAlreadyInUse(Context.Channel);
                }
            }
            else
            {
                await _messageService.AddReactionAlarm(userMsg);
                await _messageService.SendJellyfinNotAuthorize(Context.Channel, reference);
            }
            log.Info($"JellyfinAsync done");
        }


        [Command("Bug")]
        [Summary("Reboot side API service")]
        public async Task BugAsync()
        {
            SocketUserMessage userMsg = Context.Message;
            log.Info($"BugAsync by {userMsg.Author}");

            var reference = new MessageReference(userMsg.Id);
            if (Helper.IsJellyfinCorrectChannel(Context.Channel))
            {                
                string message = $" Oh lord... Something went wrong ? Sorry to hear that, there is a lot of complex communications involved in the Jellyfin process." +
                $" Hold on one sec I'll restart my side API for you, in the meantime please take a hit of weed to relax {_messageService.GetPepeSmokeEmote()} " +
                $"```Lorsque ton message aura reçu une réaction tu pourra relancer la commande $Jellyfin```";

                await Context.Channel.SendMessageAsync(text: message, messageReference: reference);

                await _jellyfinService.RestartSideApi();

                await _messageService.AddDoneReaction(userMsg);
            }
            else
            {
                await _messageService.AddReactionAlarm(userMsg);
                await _messageService.SendJellyfinNotAuthorize(Context.Channel, reference);
            }

            log.Info($"BugAsync done");
        }
        #endregion

        [Command("PurgeRoles")]
        [Summary("Purge roles that can be assigned by the bot")]
        public async Task PurgeRolesAsync()
        {
            SocketUserMessage userMsg = Context.Message;
            log.Info($"PurgeRolesAsync by {userMsg.Author}");

            var reference = new MessageReference(userMsg.Id);
            if (Helper.IsLogChannel(Context.Channel))
            {
                _roleService.PurgeRoles();                
            }
            else
            {
                await _messageService.AddReactionAlarm(userMsg);
                await _messageService.CommandNotAuthorize(Context.Channel, reference);
            }

            log.Info($"PurgeRolesAsync done");
        }
    }
}
