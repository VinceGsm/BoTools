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
		private readonly MessageService _messageService;
		private readonly JellyfinService _jellyfinService;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly string _discordImgUrl = "https://w.wallhaven.cc/full/8o/wallhaven-8o7g81.png";
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
				if (await _jellyfinService.IsLinkClear(Context.Client))
                {
                    var reference = new MessageReference(userMsg.Id);
                    await _messageService.AddReactionVu(userMsg);

                    //activation Jellyfin
                    await _jellyfinService.Activate();

                    //activation NGrock + récupération du lien http
                    string ngRockUrl = await _jellyfinService.OpenIP();
                    EmbedBuilder builder = MakeBuilder(userMsg, ngRockUrl);

                    string message = $"{_messageService.GetPepeSmokeEmote()}";

                    //await Context.Channel.SendMessageAsync(message, false, builder.Build(), null, null, reference);
                    await _messageService.JellyfinDone(userMsg);
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

        /// <summary>
        /// Message Embed with link
        /// </summary>
        /// <param name="userMsg"></param>
        /// <param name="ngRockUrl"></param>
        /// <returns></returns>
        private EmbedBuilder MakeBuilder(SocketUserMessage userMsg, string ngRockUrl)
        {            
            return new EmbedBuilder
            {
                Url = ngRockUrl,
                Color = Color.DarkRed,
                ImageUrl = _discordImgUrl,
                ThumbnailUrl = _boToolsGif,

                Title = $"{_messageService.GetCoinEmote()} Streaming & Download {_messageService.GetCoinEmote()}",
                Description = $"{_messageService.GetArrowEmote()} Ce lien ne sera disponible que pour 2h\n" +
                    $"{_messageService.GetArrowEmote()} Relancer la commande générera un nouveau lien",

                Author = new EmbedAuthorBuilder { Name = "Jellyfin requested by " + userMsg.Author.Username, IconUrl = userMsg.Author.GetAvatarUrl() },
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = Context.Guild.Owner.GetAvatarUrl(),
                    Text = $"Powered with {_messageService.GetCoeurEmoji()} by Vince"
                }
            };
        }

        // NEXT MODULE
        //1. Alerte crypto etc...		
        //2. Ventes steam de la semaine le dimanche
    }
}
