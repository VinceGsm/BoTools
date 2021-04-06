using BoTools.Service;
using Discord.Commands;
using log4net;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Module
{
    // Keep in mind your module must be public and inherit ModuleBase to be discovered by AddModulesAsync.
    [Group("Command")]
    public class MainModule : ModuleBase<SocketCommandContext>
    {
		private JellyfinService _jellyfinService;
		private MessageService _messageService;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


		[Command("Jellyfin")]
		[Summary("Active et partage un lien secret d'accès au Jellyfin privé de Vince")]
		public async Task JellyfinAsync()
		{
			log.Info($"JellyfinAsync by {Context.Message.Author}");

			await _messageService.AddReactionVu(Context.Message);

			//activation Jellyfin

			//activation NGrock + récupération du lien http

			//activer timer 2h avant delete

			//message avec lien
			// We can also access the channel from the Command Context.
			await Context.Channel.SendMessageAsync($"");
			log.Info($"JellyfinAsync done");
		}
	}
}
