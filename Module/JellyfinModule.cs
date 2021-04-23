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
				if (await _jellyfinService.IsLinkClear(Context.Client))
                {
                    var reference = new MessageReference(userMsg.Id);
                    await _messageService.AddReactionVu(userMsg);

                    //activation Jellyfin
                    _jellyfinService.Activate();

                    //activation NGrock + récupération du lien http
                    string ngRockUrl = await _jellyfinService.GetNgrokUrl();
                    EmbedBuilder builder = MakeBuilder(userMsg, ngRockUrl);

                    string message = $"{_messageService.GetPepeSmokeEmote()}";

                    await Context.Channel.SendMessageAsync(message, false, builder.Build(), null, null, reference);
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
            string vinceUrl = (Context.Channel as SocketGuildChannel)?.Guild?.Owner.GetAvatarUrl();

            return new EmbedBuilder
            {
                Url = ngRockUrl,
                Color = Color.DarkRed,
                ImageUrl = _discordImgUrl,
                ThumbnailUrl = _boToolsGif,

                Title = $"{_messageService.GetCheckEmote()}︱Streaming & Download︱{_messageService.GetCheckEmote()}",
                Description = $"{_messageService.GetCoinEmote()}  Ce lien ne sera disponible que pour 4h\n" +
                    $"{_messageService.GetCoinEmote()}  Relancer la commande générera un nouveau lien",

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
