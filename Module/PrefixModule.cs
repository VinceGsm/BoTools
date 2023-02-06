using BoTools.Service;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using log4net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Module
{
    // Your module must be public and inherit ModuleBase to be discovered by AddModulesAsync.    
    public class PrefixModule : ModuleBase<SocketCommandContext>
    {                
        private const ulong _vinceId = 312317884389130241;
        private const ulong _ordiPortableId = 493020872303443969;
        private readonly MessageService _messageService;
        private readonly EventService _eventService;
        private readonly JellyfinService _jellyfinService;        

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);        

		public PrefixModule(MessageService messageService, JellyfinService jellyfinService, EventService eventService) 
		{
			_jellyfinService = jellyfinService;
			_messageService = messageService;
            _eventService = eventService;            
        }

        
        [Command("Jellyfin")]
        [Summary("Active et partage un lien d'accès au server Jellyfin")]
        public async Task JellyfinAsync()
        {            
            List<RestMessage> pinneds = Context.Channel.GetPinnedMessagesAsync().Result.ToList();            
            var test = pinneds.First() as IUserMessage;
            if (test != null)
                await test.UnpinAsync();

            string message = string.Empty;
            SocketUserMessage userMsg = Context.Message;
                        
            userMsg.PinAsync();

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
                    _eventService.CreateNextOnePiece(true);
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

        [Command("OnePiece")]
        [Summary("Mannualy create the next Event for OnePiece streaming")]
        public async Task EventOnePieceAsync()
        {
            SocketUserMessage userMsg = Context.Message;
            log.Info($"EventOnePieceAsync by {userMsg.Author}");            

            await _eventService.CreateNextOnePiece(false);
            await _messageService.AddDoneReaction(userMsg);

            log.Info($"EventOnePieceAsync done");
        }
    }
}
