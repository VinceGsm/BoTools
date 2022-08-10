using BoTools.Service;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Module
{
    // Your module must be public and inherit ModuleBase to be discovered by AddModulesAsync.    
    public class MainModule : ModuleBase<SocketCommandContext>
    {        
        private const ulong _vinceId = 312317884389130241;
        private const ulong _PortableId = 493020872303443969;
        private readonly MessageService _messageService;
        private readonly EventService _eventService;
        private readonly JellyfinService _jellyfinService;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);        

		public MainModule(MessageService messageService, JellyfinService jellyfinService, EventService eventService) 
		{
			_jellyfinService = jellyfinService;
			_messageService = messageService;
            _eventService = eventService;
        }

        
        [Command("Jellyfin")]
        [Summary("Active et partage un lien d'accès au server Jellyfin")]
        public async Task JellyfinAsync()
        {
            string message = string.Empty;
            SocketUserMessage userMsg = Context.Message;
            log.Info($"JellyfinAsync by {userMsg.Author}");
            
            var reference = new MessageReference(userMsg.Id);
            if (Helper.IsJellyfinCorrectChannel(Context.Channel))
            {
                if (Process.GetProcessesByName("ngrok").Any())
                {
                    await _messageService.SendNgrokReset(Context.Channel);
                    await Helper.KillProcess("ngrok");
                }

                await _jellyfinService.ClearChannel(Context.Client);
                await _messageService.AddReactionVu(userMsg);

                // Jellyfin
                _jellyfinService.Activate();

                //activation NGrok + récupération du lien http
                string ngrokUrl = await _jellyfinService.GetNgrokUrl();
                log.Info($"ngrokUrl = {ngrokUrl}");

                var builder = _messageService.MakeJellyfinMessageBuilder(userMsg, ngrokUrl);
                Embed embed = builder.Build();

                if (Helper.IsSundayToday())
                {
                    message = $"{Helper.GetLuffyEmote()}";
                    _eventService.CreateNextOnePiece();
                }                    
                else
                    message = $"{Helper.GetPepeSmokeEmote()}";

                await Context.Channel.SendMessageAsync(message, false, embed, null, null, reference);
                await _messageService.AddDoneReaction(userMsg);
            }
            else
            {
                await _messageService.AddReactionAlarm(userMsg);
                await _messageService.SendJellyfinNotAuthorizeHere(Context.Channel, reference);
            }
            log.Info($"JellyfinAsync done");            
        }

        [Command("Special")]
        [Summary("Send special message in a specific channel")]
        public async Task SpecialAsync()
        {
            SocketUserMessage userMsg = Context.Message;
            log.Info($"Special by {userMsg.Author}");            

            var reference = new MessageReference(userMsg.Id);
            if (userMsg.Author.Id == _vinceId || userMsg.Author.Id == _PortableId)
            {
                _messageService.SendSpecialMessage();
            }
            else
            {
                await _messageService.AddReactionAlarm(userMsg);
                await _messageService.CommandForbidden(Context.Channel, reference);
            }

            log.Info($"Special done");
        }

        [Command("OnePiece")]
        [Summary("Create next Event for OnePiece streaming")]
        public async Task EventOnePieceAsync()
        {
            SocketUserMessage userMsg = Context.Message;
            log.Info($"EventOnePieceAsync by {userMsg.Author}");            

            await _eventService.CreateNextOnePiece();
            await _messageService.AddDoneReaction(userMsg);

            log.Info($"EventOnePieceAsync done");
        }
    }
}
