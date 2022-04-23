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
    // Your module must be public and inherit ModuleBase to be discovered by AddModulesAsync.    
    public class MainModule : ModuleBase<SocketCommandContext>
    {
        private bool _isRunning = false;
        private const ulong _vinceId = 312317884389130241;
        private const ulong _AsileId = 493020872303443969;
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

                    // Jellyfin
                    _jellyfinService.Activate();
                    log.Info($"Jellyfin activated");                    

                    //activation NGrok + récupération du lien http
                    string ngrokUrl = await _jellyfinService.GetNgrokUrl();
                    log.Info($"ngrokUrl = {ngrokUrl}");
                    _messageService.SetStatus("Jellyfin is Open !");

                    var builder = _messageService.MakeJellyfinMessageBuilder(userMsg, ngrokUrl);
                    Embed embed = builder.Build();

                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
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
                await _messageService.SendJellyfinNotAuthorizeHere(Context.Channel, reference);
            }
            log.Info($"JellyfinAsync done");
            _messageService.SetStatus(); //reset status
        }

       
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
                await _messageService.CommandNotAuthorizeHere(Context.Channel, reference);
            }

            log.Info($"PurgeRolesAsync done");
        }

        [Command("Dodo")]
        [Summary("Fait dodo")]
        public async Task DodosAsync()
        {
            SocketUserMessage userMsg = Context.Message;
            log.Info($"Dodo by {userMsg.Author}");

            var reference = new MessageReference(userMsg.Id);
            if (userMsg.Author.Id == _vinceId || userMsg.Author.Id == _AsileId)
            {
                await Helper.KillProcess("ngrok.exe");
                await Helper.KillProcess("BoTools");                          
            }
            else
            {
                await _messageService.AddReactionAlarm(userMsg);
                await _messageService.CommandForbidden(Context.Channel, reference);
            }

            log.Info($"Dodo done");
        }
    }
}
