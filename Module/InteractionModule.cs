using BoTools.Service;
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
        private const ulong _idReadRulesRole = 847048535799234560;
        private const ulong _idOpRole = 552134779210825739;
        private const ulong _idModoRole = 322489502562123778;
        private const ulong _idMemberRole = 322490732885835776;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EventService _eventService; 
        private readonly MessageService _messageService; 

        public InteractionModule(EventService eventService, MessageService messageService)
        {
            _eventService = eventService;
            _messageService = messageService;            
        }


        [RequireRole(roleId: _idReadRulesRole)]
        [SlashCommand("ping",        // Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            "Affiche la latence de BoTools",    // Descriptions can have a max length of 100.
            false, RunMode.Async)]     
        public async Task HandlePingInteraction()
        {
            //var toto = _eventService._client.GetGlobalApplicationCommandsAsync();
            //var test = toto.Result;
            //foreach (var command in test) //delete /command
            //{
            //    if (command.Name.Contains("main"))
            //        command.DeleteAsync();
            //}
            var user = Context.User;
            log.Info($"HandlePing IN by {user.Username}");

            string message = $"{Helper.GetGreeting()}```Je suis à {_eventService._client.Latency}ms de Zderland !```";

            await RespondAsync(message, ephemeral: true);
            log.Info("HandlePing OUT");
        }

        [RequireRole(roleId: _idReadRulesRole)]
        [SlashCommand("help", "Liste les commandes du server", false, RunMode.Async)]
        public async Task HandleHelpCommand()
        {
            var user = Context.User;
            log.Info($"HandleHelpCommand IN by {user.Username}");

            string description = $"{Helper._verifiedEmote} **Utility commands** {Helper._verifiedEmote}\n" +
                $"{Helper._coinEmote} </invite:1070387372824465539> : Affiche l'invitation du server\n" +
                $"{Helper._coinEmote} </ping:1009959955081728103> : Affiche le ping du server AWS\n" +
                $"{Helper._coinEmote} </roles:1069907898999767072> : Affiche la liste des rôles principaux du server\n" +                
                $"{Helper._coinEmote} </help:1092834240363778161> : Liste les commandes du server\n\n" +
                $"{Helper._verifiedEmote} **Member commands** {Helper._verifiedEmote}\n" +                
                $"{Helper._coinEmote} </anto:1122624185005518960> : Invoque un Anto aléatoire\n" +                                
                $"{Helper._coinEmote} </vocal:1172474545714757723> : Créé un vocal temporaire\n" +
                $"{Helper._coinEmote} </sondage:1122135559511494667> : Sondage dans le channel\n" +
                $"{Helper._coinEmote} </meteo_foret:1146378274709180457> : Estimation de feu de forêt en France\n" +
                $"{Helper._coinEmote} </meteo_france:1172519149327613975> : Météo d'une ville en direct\n\n" +
                $"{Helper._verifiedEmote} **OpenAI commands** {Helper._verifiedEmote}\n" +
                $"{Helper._coinEmote} </dall-e-2:1172474545714757724> Génération d'image avec la v2\n" +
                $"{Helper._coinEmote} </dall-e-3:1172474545714757725> Génération d'image avec la v3\n" +
                $"{Helper._coinEmote} </chat-gpt:1172474545714757726> Assistant basé sur la  v3.5\n\n" +
                $"{Helper._verifiedEmote} **OnePiece commands** {Helper._verifiedEmote}\n" +                
                $"{Helper._coinEmote} </feedback_one-piece:1009959955081728104>\n" +
                $"{Helper._coinEmote} </feedback_one-piece-lite:1069907898999767071>\n\n" +                
                $"En cas de problème contacter <@312317884389130241>";

            var embedBuiler = new EmbedBuilder()
                .WithTitle("Liste des commands de ZderLand :")
                .WithDescription(description)
                .WithColor(Color.Green)
                .WithThumbnailUrl(Helper._zderLandIconUrl)
                .WithImageUrl(Helper._urlListGif);

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
            log.Info("HandleHelpCommand OUT");
        }

        [RequireRole(roleId: _idReadRulesRole)]
        [SlashCommand("invite", "Affiche l'invitation éternelle du server", false, RunMode.Async)]
        public async Task HandleInviteCommand()
        {
            log.Info("HandleInviteCommand IN");

            var embedBuiler = new EmbedBuilder()
                .WithTitle("Invitation éternelle de ZderLand :")
                .WithDescription("https://discord.gg/g43kWat")
                .WithColor(Color.Green);

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
            log.Info("HandleInviteCommand OUT");
        }

        [RequireRole(roleId: _idOpRole)]
        [RequireRole(roleId: _idReadRulesRole)]
        [SlashCommand("feedback_one-piece", "Appréciation personnelle de l'épisode")]
        public async Task HandleRateOpCommand(
            [Choice("1. Mauvais !", $"💩💩💩"),
            Choice("2. Ennuyant", $"💤💤💤"),
            Choice("3. Passable", $"👁👁👁"),
            Choice("4. Sympa", $"👍👍👍"),
            Choice("5. Epoustouflant", $"🔥🔥🔥"),
            Choice("6. Légendaire !", $"❤️❤️❤️")] string feedback)
        {        
            log.Info("HandleRateOpCommand IN");

            string nameFeedback = string.Empty;
            switch (feedback)
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
                case $"👍👍👍":
                    nameFeedback = "4. Sympa";
                    break;
                case $"🔥🔥🔥":
                    nameFeedback = "5. Epoustouflant";
                    break;
                case $"❤️❤️❤️":
                    nameFeedback = "6. Légendaire !";
                    break;
            }

            string urlIcon = "";
            switch (Int32.Parse(nameFeedback.First().ToString()))
            {
                case 1:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290456782557184/vomit.png";
                    break;
                case 2:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290442140237834/bad-review.png";
                    break;
                case 3:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290427002982491/mood.png";
                    break;
                case 4:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290410867495003/good-review.png";
                    break;
                case 5:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290390063759420/satisfaction.png";
                    break;
                case 6:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290172836552844/heart.png";
                    break;
            }
            log.Info("urlIcon : " + urlIcon);

            var embedBuilder = new EmbedBuilder()
                
                .WithTitle($"Feedback : {nameFeedback.Remove(0, 3)}")
                //.WithDescription($"{feedback}")
                .WithColor(Color.DarkBlue)
                .WithImageUrl(urlIcon);

            await RespondAsync(embed: embedBuilder.Build());
            log.Info("HandleRateOpCommand OUT");
        }

        [RequireRole(roleId: _idOpRole)]
        [RequireRole(roleId: _idReadRulesRole)]
        [SlashCommand("feedback_one-piece-lite", "Appréciation personnelle de l'épisode")]
        public async Task HandleRateOpCommandLite(
            [Choice("1. Nullissime", "⭐"),
            Choice("2. Pas ouf", "⭐⭐"),
            Choice("3. Ok", "⭐⭐⭐"),
            Choice("4. Bien", "⭐⭐⭐⭐"),
            Choice("5. Excellent", "⭐⭐⭐⭐⭐")            
            ] string feedback) 
        {
            log.Info("HandleRateOpLiteCommand IN");

            string nameFeedBack = string.Empty;

            switch (feedback)
            {
                case $"⭐":
                    nameFeedBack = "NULLISSIME";
                    break;
                case $"⭐⭐":
                    nameFeedBack = "PAS OUF";
                    break;
                case $"⭐⭐⭐":
                    nameFeedBack = "OK";
                    break;
                case $"⭐⭐⭐⭐":
                    nameFeedBack = "BIEN";
                    break;
                case $"⭐⭐⭐⭐⭐":
                    nameFeedBack = "EXCELLENT";
                    break;
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Feedback : {nameFeedBack}")
                .WithDescription($"{feedback}")
                .WithColor(Color.DarkRed);

            await RespondAsync(embed: embedBuilder.Build());
            log.Info("HandleRateOpLiteCommand OUT");
        }

        [RequireRole(roleId: _idReadRulesRole)]
        [SlashCommand("roles", "Affiche la liste des rôles principaux du server", false, RunMode.Async)]        
        public async Task HandleMainRolesCommand()
        {
            var user = Context.User;
            log.Info($"HandleMainRolesCommand IN by {user.Username}"); 

            List<string> roles = new List<string>(){ "<@&322489502562123778>", "<@&322490732885835776>",
            "<@&344912149728067584>","<@&847048535799234560>"};

            // We remove the everyone role and select the mention of each role.
            var roleList = string.Join("\n", roles);

            var embedBuiler = new EmbedBuilder()                
                .WithTitle("Rôles principaux de Zderland :")
                .WithThumbnailUrl(Helper._zderLandIconUrl)
                .WithDescription(roleList)
                .WithColor(Color.Green);                

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
            log.Info("HandleMainRolesCommand OUT");
        }

        [RequireRole(roleId: _idModoRole)]
        [SlashCommand("msg-new-roles", "Annonce dans le général les nouveux roles", true, RunMode.Async)]        
        public async Task HandleMsgNewRolesCommand(string roles)
        {
            log.Info("HandleMsgNewRolesCommand IN");            

            var roleList = roles.Replace(" ","\n");

            string msg = $"Salutations {Helper._coinEmote}\n" +
                "Voici la liste des rôles fraîchement ajoutés au server :\n" + roleList;

            await Context.Channel.SendMessageAsync(msg);

            log.Info("HandleMsgNewRolesCommand OUT");
        }

        [RequireRole(roleId: _idModoRole)]
        [SlashCommand("event-serie-hebdo", "Créé tout les events pour des episodes hebdo", true, RunMode.Async)] 
        public async Task HandleEventSeriesHebdoCommand(string name, int numFirstEpisode, int numLastEpisode, DayOfWeek dayOfWeek, Double hour)
        {
            log.Info("HandleEventSeriesHebdoCommand IN");

            var user = Context.User;
            string msg = "N'oubliez pas de cliqué sur la cloche de l'event afin d'être notifié lorsqu'il commence !";               

            int nbEp = numLastEpisode - numFirstEpisode +1;

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithTitle($"Création de {nbEp} events {name} en cours...")
                .WithDescription(msg)
                .WithColor(Color.Green);                          

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: false);

            _eventService.CreateEventHebdoSerie(name, numFirstEpisode, nbEp, dayOfWeek, hour);

            log.Info("HandleEventSeriesHebdoCommand OUT");
        }

        [RequireRole(roleId: _idModoRole)]
        [SlashCommand("event-enserie", "Créé X event : selectionner les jours où auront lieu cet event", true, RunMode.Async)]
        public async Task HandleEventSeriesCommand(string name, int nbSession, Double hour, bool isIrlEvent,
        DayOfWeek? siLundi=null, DayOfWeek? siMardi=null, DayOfWeek? siMercredi=null, DayOfWeek? siJeudi=null, DayOfWeek? siVendredi=null, DayOfWeek? siSamedi=null, DayOfWeek? siDimanche=null)        
        {
            log.Info("HandleEventSeriesCommand IN");

            var user = Context.User;
            string msg = "N'oubliez pas de cliqué sur la cloche de l'event afin d'être notifié lorsqu'il commence !";

            var embedBuiler = new EmbedBuilder()
                .WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithTitle($"Création de {nbSession} events {name} en cours...")
                .WithDescription(msg)
                .WithColor(Color.Green);                

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: false);            

            _eventService.CreateEventEnSerie(name, hour, nbSession, isIrlEvent,
                siLundi,siMardi,siMercredi,siJeudi,siVendredi,siSamedi,siDimanche);

            log.Info("HandleEventSeriesCommand OUT");
        }

        [RequireRole(roleId: _idModoRole)]
        [SlashCommand("create-onepiece", "Créé thread + event du prochain épisode si manquant", true, RunMode.Async)]
        public async Task HandleCreateOnePieceCommand()
        {
            RespondAsync(text: "I'm on it captain !", ephemeral: true);
            await _eventService.CreateNextOnePiece();
            
            log.Info("HandleCreateOnePieceCommand OUT");
        }

        [RequireRole(roleId: _idMemberRole)]
        [SlashCommand("vocal", "Créé un vocal temporaire pour le nombre de participant souhaité", true, RunMode.Async)]
        public async Task HandleCreateVocalReuCommand(string theme, int nbParticipant)
        {
            var embedBuiler = _messageService.CreateVocalReu(theme, nbParticipant);

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: true);

            log.Info("HandleCreateVocalReuCommand OUT");
        }

        [RequireRole(roleId: _idMemberRole)]
        [SlashCommand("sondage", "Créé un sondage dans le channel utilisé (Mettre un . pour les opt non-utilisés)", true, RunMode.Async)]
        public async Task HandleVoteCommand(string question, string optA, string optB, string optC, string optD, string optE)
        {
            //🤍blanc 💜violet 💚vert 💛jaune 🧡orange
            List<string> emojisStr = new List<string> { "🤍", "💜", "💚", "💛", "🧡" };
            List<Emoji> emojis = new List<Emoji> { new Emoji("\U0001f90d"), new Emoji("\U0001f49c") };

            List<string> options = new List<string>(){ optA,optB};
            if (optC != ".") { options.Add(optC); emojis.Add(new Emoji("\U0001f49A")); }
            if (optD != ".") { options.Add(optD); emojis.Add(new Emoji("\U0001f49b")); }
            if (optE != ".") { options.Add(optE); emojis.Add(new Emoji("\U0001f9e1")); }

            var embedBuiler = _messageService.CreateVote(question, options, emojisStr);

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: false);
            var msg = (IMessage)Context.Channel.GetMessagesAsync(1).ToListAsync().Result.First().First();
            await _messageService.AddVoteEmoji(msg, emojis);

            log.Info("HandleVoteCommand OUT");
        }

        [RequireRole(roleId: _idMemberRole)]
        [SlashCommand("anto", "Invoque un Anto aléatoire", true, RunMode.Async)]
        public async Task HandleAntoCommand()
        {
            var embedBuiler = _messageService.CreateAntoEmbed();

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: false);

            log.Info("HandleAntoCommand OUT");
        }

        [RequireRole(roleId: _idMemberRole)]
        [SlashCommand("meteo_foret", "Estimation de feu de forêt en France pour aujourd'hui et demain", true, RunMode.Async)]
        public async Task HandleMeteoForetCommand()
        {
            log.Info("HandleMeteoForetCommand IN");

            if (DateTime.Now.Month >= 6 && DateTime.Now.Month <= 9) //Juin à Septembre (inclut)
            {
                RespondAsync(text: $"La suite dans quelques temps dans <#{Helper._idThreadMeteo}>", ephemeral: true);

                await _messageService.SendMeteoForetEmbed();
            }
            else
                RespondAsync(text: $"Cette fonctionnalité n'est disponible qu'entre Juin et Septembre.", ephemeral: true);

            log.Info("HandleMeteoForetCommand OUT");
        }

        [RequireRole(roleId: _idMemberRole)]
        [SlashCommand("meteo_france", "Météo d'une ville en direct", true, RunMode.Async)]
        public async Task HandleMeteoCommand(string ville)
        {
            log.Info("HandleMeteoCommand IN");

            RespondAsync(text: $"La suite dans quelques temps dans <#{Helper._idThreadMeteo}>", ephemeral: true);            

            await _messageService.SendMeteoEmbed(ville);

            log.Info("HandleMeteoCommand OUT");
        }

        [RequireRole(roleId: _idMemberRole)]
        [SlashCommand("dall-e-2", "Ask Dall-E-2 for an image [ENGLISH]")]        
        public async Task HandleDallE2(string prompt)
        {
            log.Info("HandleDalle IN");

            if (Context.Channel.Id != 1171768483810390027)
            {
                await RespondAsync(text: "Merci d'utiliser cette commande dans <#1171768483810390027> avec une query respectant " +
                    "la [politique d'usage](https://openai.com/policies/usage-policies)", ephemeral:true);
            }
            else
            {
                string userToken = Helper.GetOpenAIToken(Context.User.Id);

                if (!string.IsNullOrEmpty(userToken))
                {
                    await RespondAsync(text: "Laisse moi 15sec le temps d'aller télécharger ton image chez l'cousin Dall-E", ephemeral: true);
                    await _messageService.QueryDallE(2, userToken, prompt, false, Context);                    
                }
                else
                {
                    await RespondAsync(text: "Mes circuits ne détectent aucune token API_OpenAI pour ce compte Discord.\n" +
                        "Votre premier token généré est gratuit et vous donne 5$ d'utilisation. Intéressé? Contact <@312317884389130241> pour la modique somme de 0€", ephemeral: true);
                }
            }

            log.Info("HandleDalle OUT");
        }

        [RequireRole(roleId: _idMemberRole)]
        [SlashCommand("dall-e-3", "Ask Dall-E-3 for an image [ENGLISH]")]
        public async Task HandleDallE3(string prompt, bool HD)
        {
            log.Info("HandleDalle IN");

            if (Context.Channel.Id != 1171768483810390027)
            {
                await RespondAsync(text: "Merci d'utiliser cette commande dans <#1171768483810390027> avec une query respectant " +
                    "la [politique d'usage](https://openai.com/policies/usage-policies)", ephemeral: true);
            }
            else
            {
                string userToken = Helper.GetOpenAIToken(Context.User.Id);

                if (!string.IsNullOrEmpty(userToken))
                {
                    await RespondAsync(text: "Laisse moi 15sec le temps d'aller télécharger ton image chez l'cousin Dall-E", ephemeral: true);
                    await _messageService.QueryDallE(3, userToken, prompt, HD, Context);
                }
                else
                {
                    await RespondAsync(text: "Mes circuits ne détectent aucune token API_OpenAI pour ce compte Discord.\n" +
                        "Votre premier token généré est gratuit et vous donne 5$ d'utilisation. Intéressé? Contact <@312317884389130241> pour la modique somme de 0€", ephemeral:true);
                }
            }

            log.Info("HandleDalle OUT");
        }

        [RequireRole(roleId: _idMemberRole)]
        [SlashCommand("chat-gpt", "Ask GPT-3.5_Turbo anything [ENGLISH]")]
        public async Task HandleChatGpt(string prompt)
        {
            log.Info("HandleChatGpt IN");

            if (Context.Channel.Id != 1171768653012803634)
            {
                await RespondAsync(text: "Merci d'utiliser cette commande dans <#1171768653012803634> avec une query respectant " +
                    "la [politique d'usage](https://openai.com/policies/usage-policies)", ephemeral: true);
            }
            else
            {
                string userToken = Helper.GetOpenAIToken(Context.User.Id);

                if (!string.IsNullOrEmpty(userToken))
                {
                    await RespondAsync(text: "En attente de réponse du collègue GPT-3.5_Turbo", ephemeral: true);
                    await _messageService.QueryChatGpt(userToken, prompt, Context.User);
                }
                else
                {
                    await RespondAsync(text: "Mes circuits ne détectent aucune token API_OpenAI pour ce compte Discord.\n" +
                        "Votre premier token généré est gratuit et vous donne 5$ d'utilisation. Intéressé? Contact <@312317884389130241> pour la modique somme de 0€", ephemeral: true);
                }
            }                        

            log.Info("HandleChatGpt OUT");
        }
    }
}
