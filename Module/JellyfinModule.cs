using BoTools.Service;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Module
{
    // Keep in mind your module must be public and inherit ModuleBase to be discovered by AddModulesAsync.    
    public class JellyfinModule : ModuleBase<SocketCommandContext>
    {
        private bool _isRunning = false;
        private readonly MessageService _messageService;
		private readonly JellyfinService _jellyfinService;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);        
		private static readonly string _discordImgUrl = "https://media.discordapp.net/attachments/617462663374438411/835124361249161227/unknown.png";
		private static readonly string _boToolsGif = "https://cdn.discordapp.com/attachments/617462663374438411/830856271321497670/BoTools.gif";


		public JellyfinModule(MessageService messageService, JellyfinService jellyfinService) 
		{
			_jellyfinService = jellyfinService;
			_messageService = messageService;
		}

		[Command("Jellyfin")]
		[Summary("Active et partage un lien secret d'accès au Jellyfin privé de Vince")]
		public async Task JellyfinAsync()
		{
			SocketUserMessage userMsg = Context.Message;
			log.Info($"JellyfinAsync by {userMsg.Author}");

            if (Helper.IsJellyfinCorrectChannel(Context.Channel))
            {
				if (!_isRunning)
                {
                    await _jellyfinService.ClearChannel(Context.Client);
                    var reference = new MessageReference(userMsg.Id);
                    await _messageService.AddReactionVu(userMsg);

                    //activation Jellyfin
                    _jellyfinService.Activate();
                    log.Info($"Jellyfin activated");

                    //activation NGrock + récupération du lien http
                    string ngrokUrl = await _jellyfinService.GetNgrokUrl();
                    log.Info($"ngrokUrl = {ngrokUrl}");

                    var builder = MakeBuilder(userMsg, ngrokUrl);
                    Embed embed = builder.Build();

                    string message = $"{_messageService.GetPepeSmokeEmote()}";
                    
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
				await _messageService.SendJellyfinNotAuthorize(Context.Channel);
			}				
			log.Info($"JellyfinAsync done");
		}


        [Command("Bug")]
        [Summary("Reboot side API service")]
        public async Task BugAsync()
        {
            SocketUserMessage userMsg = Context.Message;
            log.Info($"BugAsync by {userMsg.Author}");

            if (Helper.IsJellyfinCorrectChannel(Context.Channel))
            {
                var reference = new MessageReference(userMsg.Id);

                string message = $" Oh lord... Something went wrong ? Sorry to hear that, there is a lot of complex communications involved in the Jellyfin process." +
                $" Hold on one sec I'll restart my side API for you, in the meantime please take a hit of weed to relax {_messageService.GetPepeSmokeEmote()} " +
                $"```Lorsque ton message aura reçu une réaction tu pourra relancer la commande $Jellyfin```";

                await Context.Channel.SendMessageAsync(text:message, messageReference:reference);

                await _jellyfinService.RestartSideApi();                

                await _messageService.AddDoneReaction(userMsg);
            }
            else
            {
                await _messageService.AddReactionAlarm(userMsg);
                await _messageService.SendJellyfinNotAuthorize(Context.Channel);
            }

            log.Info($"BugAsync done");
        }



        /// <summary>
        /// Message Embed with link
        /// </summary>
        /// <param name="userMsg"></param>
        /// <param name="ngRockUrl"></param>
        /// <returns></returns>
        private EmbedBuilder MakeBuilder(SocketUserMessage userMsg, string ngRockUrl)
        {            
            string vinceUrl = (Context.Channel as SocketGuildChannel)?.Guild?.Owner.GetAvatarUrl();

            return new EmbedBuilder
            {
                Url = ngRockUrl,
                Color = Color.DarkRed,
                ImageUrl = _discordImgUrl,
                ThumbnailUrl = _boToolsGif,

                Title = $"{_messageService.GetCheckEmote()}︱Streaming & Download︱{_messageService.GetCheckEmote()}",
                Description = $"{_messageService.GetCoinEmote()}  Relancer **$Jellyfin** si le lien ne fonctionne plus\n" +
                    $"{_messageService.GetCoinEmote()}  En cas de problème : **$BUG**",

                Author = new EmbedAuthorBuilder { Name = "Jellyfin requested by " + userMsg.Author.Username, IconUrl = userMsg.Author.GetAvatarUrl() },
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = vinceUrl, 
                    Text = $"Powered with {_messageService.GetCoeurEmoji()} by Vince"
                }
            };
        }       
    }
}
