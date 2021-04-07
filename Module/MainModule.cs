using BoTools.Service;
using Discord.Commands;
using log4net;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Module
{
    // Keep in mind your module must be public and inherit ModuleBase to be discovered by AddModulesAsync.    
    public class MainModule : ModuleBase<SocketCommandContext>
    {
		private readonly MessageService _messageService;
		private readonly JellyfinService _jellyfinService;		
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


		public MainModule(MessageService messageService, JellyfinService jellyfinService) 
		{
			_jellyfinService = jellyfinService;
			_messageService = messageService;
		}

		[Command("Jellyfin")]
		[Summary("Active et partage un lien secret d'accès au Jellyfin privé de Vince")]
		public async Task JellyfinAsync()
		{
			log.Info($"JellyfinAsync by {Context.Message.Author}");

            if (false)
            {
				await _messageService.AddReactionVu(Context.Message);

				//activation Jellyfin

				//activation NGrock + récupération du lien http

				//activer timer 2h avant delete

				//message avec lien
				// We can also access the channel from the Command Context.
				await Context.Channel.SendMessageAsync($"hello");
			}
            else
            {
				await _messageService.AddReactionRefused(Context.Message);
				await _messageService.SendJellyfinRefused(Context.Channel);
			}

			log.Info($"JellyfinAsync done");
		}

		//Quoi 2 9 ? --> Ventes steam + countdown prochain EP OP
	}
}
