using Discord;
using Discord.Interactions;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Module
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong _idOpRole = 552134779210825739;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public InteractionModule()
        {
        }
        
        
        [SlashCommand("ping",        // Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            "BoTools es-tu là ?",    // Descriptions can have a max length of 100.
            false, RunMode.Async)]     
        public async Task HandlePingPongInteraction()
        {                       
            await RespondAsync("PONG !");            
        }


        [RequireRole(roleId: _idOpRole)]
        [SlashCommand("feedback_one-piece", "Comment as-tu trouver le dernier épisode de One Piece ?")]
        public async Task HandleRateOpCommand(
            [Choice("1. Mauvais !", $"💩💩💩"),
            Choice("2. Ennuyant", $"💤💤💤"),
            Choice("3. Passable", $"👁👁👁"),
            Choice("4. Régale", $"🔥🔥🔥"),
            Choice("5. Epoustouflant", $"⭐️⭐️⭐️"),
            Choice("6. Légendaire !", $"❤️❤️❤️")] string feeback)
        {                                 
            string nameFeedback = string.Empty;
            switch (feeback)
            {
                case $"💩💩💩":
                    nameFeedback = "1. Mauvais !";
                    break;
                case $"💤💤💤":
                    nameFeedback = "2. Ennuyant";
                    break;
                case $"👁👁👁":
                    nameFeedback = "3. Passable";
                    break;
                case $"🔥🔥🔥":
                    nameFeedback = "4. Régale";
                    break;
                case $"⭐️⭐️⭐️":
                    nameFeedback = "5. Epoustouflant";
                    break;
                case $"❤️❤️❤️":
                    nameFeedback = "6. Légendaire !";
                    break;
            }

            string urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1007316947135889418/unknown.png"; //middle by default
            switch (Int32.Parse(nameFeedback.First().ToString()))
            {
                case 1:
                case 2:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1007316947513380914/unknown.png";
                    break;
                case 5:
                case 6:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1007316948578730015/unknown.png";
                    break;
            }

            var embedBuilder = new EmbedBuilder()
                //.WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithTitle($"Feedback : {nameFeedback.Remove(0, 3)}")
                .WithDescription($"{feeback}")
                .WithColor(Color.Red)
                .WithImageUrl(urlIcon);

            await RespondAsync(embed: embedBuilder.Build());
        }


        [SlashCommand("main-roles", "Affiche la liste des rôles principaux du server", false, RunMode.Async)]        
        public async Task HandleMainRolesCommand()
        {
            List<string> roles = new List<string>();
            roles.Add("<@&689144324939710527>");
            roles.Add("<@&322489502562123778>");
            roles.Add("<@&322490732885835776>");
            roles.Add("<@&344912149728067584>");
            roles.Add("<@&847048535799234560>");

            // We remove the everyone role and select the mention of each role.
            var roleList = string.Join("\n", roles);            

            var embedBuiler = new EmbedBuilder()
                //.WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithTitle("Rôles principaux de Zderland :")
                .WithDescription(roleList)
                .WithColor(Color.Green)
                .WithImageUrl(Helper.GetZderLandIconUrl());

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
        }
    }
}
