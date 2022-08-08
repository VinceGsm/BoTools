using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class MessageService
    {       
        private DiscordSocketClient _client;
        private ISocketMessageChannel _logChannel;        
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MessageService(DiscordSocketClient client)
        {
            _client = client;                                   
            _client.Ready += Ready;
            _client.UserLeft += UserLeft;                                  
            _client.MessageReceived += MessageReceived;
        }
        
        #region Client
        /// <summary>
        /// When guild data has finished downloading (+state : Ready)
        /// </summary>
        /// <returns></returns>
        public async Task Ready()
        {            
            await SendLatencyAsync();
            await CheckBirthday();            
            await _client.DownloadUsersAsync(_client.Guilds); // DL all user
        }

        public async Task UserJoined(SocketGuildUser guildUser)
        {            
            if (!guildUser.IsBot)
            {                
                var msg = $"Je t'invite à prendre quelques minutes pour lire les règles du serveur sur le canal textuel <#846694705177165864>\n" +                    
                    $"En cas de problème merci de contacter **Vince#0420**\n" +
                    $"A très vite pour de nouvelles aventures sur ZderLand {Helper.GetCoeurEmote()}" ;

                var builder = MakeMessageBuilder(guildUser);
                Embed embed = builder.Build();

                string message = $"{Helper.GetPikachuEmote()}";

                await guildUser.SendMessageAsync(text:message, false, embed:embed, null, null);
                await guildUser.SendMessageAsync(msg);
            }
            return;
        }

        /// <summary>
        /// When a User left the Guild
        /// </summary>
        /// <param name="guildUser"></param>
        /// <returns></returns>
        //private async Task UserLeft(SocketGuildUser guildUser) 
        private async Task UserLeft(SocketGuild arg1, SocketUser guildUser)
        {
            log.Warn($"{guildUser.Username} left");                                                     
            string message = $"<@{guildUser.Id}> left Zderland !";             

            if (_logChannel != null)
                await _logChannel.SendMessageAsync(message);

            return;
        }

        private Task MessageReceived(SocketMessage arg)
        {
            //DM from User
            if (arg.Source == MessageSource.User && arg.Channel.Name.StartsWith('@'))
            {
                string message = $"<@{arg.Author.Id}> *says* : " + arg.Content ;
                SendToLeader(message);
                AddReactionRobot((SocketUserMessage)arg);
            }
                
            return Task.CompletedTask;
        }
        #endregion

        #region Reaction
        public async Task AddReactionVu(SocketUserMessage message)
        {
            // --> 👀
            Emoji vu = new Emoji("\uD83D\uDC40");
            await message.AddReactionAsync(vu);
        }

        public async Task AddReactionRefused(SocketUserMessage message)
        {
            // --> ❌
            Emoji cross = new Emoji("\u274C");
            await message.AddReactionAsync(cross);
        }

        public async Task AddReactionRobot(SocketUserMessage message)
        {
            // --> 🤖
            Emoji robot = new Emoji("\uD83E\uDD16");
            await message.AddReactionAsync(robot);
        }

        public async Task AddReactionAlarm(SocketUserMessage message)
        {            
            var alarm = Emote.Parse(Helper.GetAlarmEmote()) ;            
            await message.AddReactionAsync(alarm);
        }

        public async Task AddReactionBirthDay(IMessage message)
        {            
            var bravo = Emote.Parse(Helper.GetBravoEmote());
            // --> 🎂
            Emoji cake = new Emoji("\uD83C\uDF82");

            await message.AddReactionAsync(cake);
            await message.AddReactionAsync(bravo);
        }

        public async Task AddDoneReaction(SocketUserMessage message)
        {
            await message.RemoveAllReactionsAsync();

            var check = Emote.Parse(Helper.GetDoneEmote());
            await message.AddReactionAsync(check);
        }
        #endregion

        #region Message
        public async Task SendLatencyAsync()
        {   
            _logChannel = Helper.GetSocketMessageChannel(_client, 826144013920501790);

            IAsyncEnumerable<IReadOnlyCollection<IMessage>> lastMsgAsync = _logChannel.GetMessagesAsync(1); 
            var lastMsg = lastMsgAsync.FirstAsync().Result;
            bool newLog = lastMsg.ElementAt(0).Timestamp.Day != DateTimeOffset.Now.Day;

            if (newLog)
            {
                string message = $"{Helper.GetGreeting()}```Je suis à {_client.Latency}ms de Zderland !```";

                if (_logChannel != null)
                    await _logChannel.SendMessageAsync(message, isTTS: true);
            }
            
            log.Info($"Latency : {_client.Latency} ms");
        }

        private async Task CheckBirthday()
        {
            bool isAlreadyDone = false;
            string msgStart = $"@here {Helper.GetPikachuEmote()} \n" +
                        $"On me souffle dans l'oreille que c'est l'anniversaire de";

            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, Helper._idGeneralChannel);
            IAsyncEnumerable<IReadOnlyCollection<IMessage>> msg = channel.GetMessagesAsync(99);
            var msgAsync = msg.ToListAsync().Result;

            foreach (var list in msgAsync)
            {
                IMessage message = list.First(x => x.Content.StartsWith(msgStart));
                
                if (message.Author.IsBot && (message.CreatedAt.Day == DateTime.Today.Day))
                {
                    isAlreadyDone = true;
                    break;
                }                    
            }

            if (!isAlreadyDone)
            {
                Dictionary<string, DateTime> birthsDay = Helper.GetBirthsDay();
                bool isSomeoneBD = birthsDay.ContainsValue(DateTime.Today);

                if (isSomeoneBD)
                {
                    string id = birthsDay.First(x => x.Value == DateTime.Today).Key;

                    string message = msgStart + $" <@{id}> aujourd'hui !\n" +                    
                    $"{Helper.GetCoeurEmote()}";

                    if (channel != null)
                    {
                        var res = (IMessage)channel.SendMessageAsync(message).Result;
                        await AddReactionBirthDay(res);
                    }
                    else log.Error("Can't wish HB because general was not found");
                }
            }                        
        }

        internal async void SendSpecialMessage(ISocketMessageChannel channel)
        {
            //ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, _idChannelGeneral);
            await channel.SendMessageAsync($"Salutations <@&816282726654279702> !\n\n");
            string msg =                 
                $"**J'ai le plaisir de vous annoncer la version 2 du service Jellyfin de Zderland** {Helper.GetPepeSmokeEmote()}\n\n" + 
                $"{Helper.GetCoinEmote()} Un server maison a été mit en place afin de permettre une disponibilité du service **24h/24h**\n" +
                $"Cela veut aussi dire que les liens générés par mes soins seront actifs de manière *casi* permanente\n" +
                $"{Helper.GetArrowEmote()} ||Donc pensez à vérifier le dernier lien présent avant d'en regénérer un autre pour ne pas couper l'accès Jellyfin à quelqu'un {Helper.GetHeheEmote()}||\n" +
                $"{Helper.GetCoinEmote()} Un NAS maison a aussi été mit en place afin de garantir un espace de stockage croissant au fil du temps !\n" +                
                $"{Helper.GetCoinEmote()} Une nouvelle version de Jellyfin a été installé (et testé avec l'aide de xxxxxx) ce qui inclut : \n" +
                $"```- Chapitrage illustré des épisodes qui le permettent\n" +
                $"- Moins de bug {Helper.GetTvEmoji()}\n" +
                $"- SyncPlay de nouveau fonctionnel \n" +
                $"- Nouveau compte pour tous les anciens + compte invité\n" +
                $"- Jusqu'à 3 flux de streaming en 4K simultanés\n" +
                $"- Jusqu'à 5 flux de streaming en 1080p simultanés```";
            await channel.SendMessageAsync(msg);

            string msg2 = $"Si vous souhaitez essayer le service mais que vous n'avez pas encore accès à <#{Helper._idJellyfinChannel}> contactez un <@&{Helper._idModoRole}>\n" +
                $"{Helper.GetPikachuEmote()}";
            await channel.SendMessageAsync(msg2);
        }

        internal void OnePieceDispo()
        {
            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, Helper._idJellyfinChannel);                        

            channel.SendMessageAsync(Helper.GetOnePieceMessage());
        }

        internal void SendToLeader(string message)
        {
            var leader = _client.GetUser(312317884389130241);
            leader.SendMessageAsync(message);            
        }

        #region Control Message
        internal async Task CommandNotAuthorizeHere(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"L'utilisation de cette commande est limitée au channel <#826144013920501790>", messageReference: reference);
        }

        internal async Task CommandForbidden(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"L'utilisation de cette commande est interdite !", messageReference: reference);
        }

        internal async Task SendJellyfinNotAuthorizeHere(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"⚠️ Pour des raisons de sécurité l'utilisation de Jellyfin" +
                $" est limitée au channel <#816283362478129182>", messageReference: reference);            
        }

        internal async Task SendNgrokReset(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync($"{Helper.GetAlarmEmote()} Un nouveau lien va être généré ! {Helper.GetAlarmEmote()}\n" +
                $"En cas de soucis direct avec Jellyfin merci de contacter Vince");            
        }
        #endregion
        #endregion        

        #region Embed
        /// <summary>
        /// Message Embed with link
        /// </summary>
        /// <param name="userMsg"></param>
        /// <param name="ngRockUrl"></param>
        /// <returns></returns>
        public EmbedBuilder MakeJellyfinMessageBuilder(SocketUserMessage userMsg, string ngRockUrl)
        {                        
            return new EmbedBuilder
            {
                Url = ngRockUrl,
                Color = Color.DarkRed,
                ImageUrl = Helper._discordImgUrl,
                ThumbnailUrl = Helper._boToolsGif,

                Title = $"{Helper.GetCheckEmote()}︱Cliquez ici︱{Helper.GetCheckEmote()}",                
                Description = $"{Helper.GetCoinEmote()}  À utiliser avec **Google Chrome** | **Firefox** | **Safari** \n" +
                    $"{Helper.GetCoinEmote()}  Relancer **$Jellyfin** si le lien ne fonctionne plus",

                Author = new EmbedAuthorBuilder { Name = "Jellyfin requested by " + userMsg.Author.Username, IconUrl = userMsg.Author.GetAvatarUrl() },
                Footer = GetFooterBuilder()
            };
        }

        private EmbedBuilder MakeMessageBuilder(SocketGuildUser guildUser)
        {                        
            EmbedBuilder res = new EmbedBuilder
            {                
                Color = Color.DarkRed,                
                ThumbnailUrl = Helper._boToolsGif,

                Title = $"{Helper.GetCheckEmote()}︱WELCOME︱{Helper.GetCheckEmote()}",
                Description = $"Bienvenue sur Zderland {guildUser.Username} !",     

                Author = new EmbedAuthorBuilder { Name = "Mes circuits ont détectés l'arrivée de " + guildUser.Username, IconUrl = guildUser.GetAvatarUrl() },
                Footer = GetFooterBuilder()
            };
            return res;
        }

        private EmbedFooterBuilder GetFooterBuilder()
        {
            return new EmbedFooterBuilder
            {
                IconUrl = Helper._urlAvatarVince,
                Text = $"Powered with {Helper.GetCoeurEmoji()} by Vince"
            };
        }
        #endregion
    }
}