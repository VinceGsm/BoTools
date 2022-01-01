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
        private static ulong _idChannelGeneral = 312966999414145034;
        private static ulong _idJellyfinChannel = 816283362478129182;
        private static string _eternalInvite = "https://discord.gg/g43kWat";
        #region emote                
        private static readonly string _coinEmote = "<a:Coin:637802593413758978>";
        private static readonly string _doneEmote = "<a:check:626017543340949515>";
        private static readonly string _arrowEmote = "<a:arrow:830799574947463229>";
        private static readonly string _alarmEmote = "<a:alert:637645061764415488>";
        private static readonly string _coeurEmote = "<a:coeur:830788906793828382>";
        private static readonly string _bravoEmote = "<a:bravo:626017180731047977>";
        private static readonly string _luffyEmote = "<a:luffy:863101041498259457>";
        private static readonly string _checkEmote = "<a:verified:773622374926778380>";        
        private static readonly string _catVibeEmote = "<a:catvibe:792184060054732810>";
        private static readonly string _pikachuEmote = "<a:hiPikachu:637802627345678339>";
        private static readonly string _pepeSmokeEmote = "<a:pepeSmoke:830799658354737178>";  
        #endregion
        #region emoji
        private static readonly string _coeurEmoji = "\u2764";        
        private static readonly string _tvEmoji = "\uD83D\uDCFA";
        private static readonly string _dlEmoji = "<:DL:894171464167747604>";
        #endregion        
        private DiscordSocketClient _client;
        private ISocketMessageChannel _logChannel;        
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string _discordImgUrl = "https://cdn.discordapp.com/attachments/617462663374438411/863110514199494656/5ffdaa1e9978e227df8b2e2f.webp";
        private static readonly string _boToolsGif = "https://cdn.discordapp.com/attachments/617462663374438411/830856271321497670/BoTools.gif"; 
        private static readonly string _urlAvatarVince = "https://cdn.discordapp.com/attachments/617462663374438411/846821971114983474/luffy.gif"; 


        public MessageService(DiscordSocketClient client)
        {
            _client = client;                                   
            _client.Ready += Ready;
            _client.UserLeft += UserLeft;                      
            _client.InviteCreated += InviteCreated;
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
                    $"En cas de problème merci de contacter *Vince#0420*\n" +
                    $"A très vite pour de nouvelles aventures {_coeurEmote}" ;

                var builder = MakeMessageBuilder(guildUser);
                Embed embed = builder.Build();

                string message = $"{_pikachuEmote}";

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

        private Task InviteCreated(SocketInvite invite)
        {            
            var inviter = _client.GetUser(invite.Inviter.Id);            
            //IChannel channel = Helper.GetSocketChannel(_client, invite.Channel.Name);            

            string duration = (invite.IsTemporary) ? "éternelle" : $"valable {invite.MaxAge/3600}h";

            string logMessage = $"Une nouvelle invitation (*{duration}*) vient d'être créée par " +
                $"<@{inviter.Id}> dans : {invite.Channel.Name} | #{invite.ChannelId}";

            string message = $"{_alarmEmote} Voici l'invitation officielle de ZderLand à partager : {_eternalInvite}\n" +
                $"Merci à toi, la bise {_coeurEmote}";

            SendToLeader(logMessage);
            inviter.SendMessageAsync(message);

            return Task.CompletedTask;           
        }

        private Task MessageReceived(SocketMessage arg)
        {
            //DM from User
            if (arg.Source == MessageSource.User && arg.Channel.Name.StartsWith('@'))
            {
                string message = $"<@{arg.Author.Id}> *says* : " + arg.Content ;
                SendToLeader(message);
            }
                
            return Task.CompletedTask;
        }

        public void SetStatus(string text = null)
        {
            //message de base
            _client.SetGameAsync(name: text ?? ": $Jellyfin", streamUrl: Helper.statusLink, type: ActivityType.CustomStatus);
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
            var alarm = Emote.Parse(_alarmEmote) ;            
            await message.AddReactionAsync(alarm);
        }

        public async Task AddReactionBirthDay(IMessage message)
        {            
            var bravo = Emote.Parse(_bravoEmote);
            // --> 🎂
            Emoji cake = new Emoji("\uD83C\uDF82");

            await message.AddReactionAsync(cake);
            await message.AddReactionAsync(bravo);
        }

        internal async Task AddDoneReaction(SocketUserMessage message)
        {
            await message.RemoveAllReactionsAsync();

            var check = Emote.Parse(_doneEmote);
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
            string msgStart = $"@everyone {_pikachuEmote} \n" +
                        $"On me souffle dans l'oreille que c'est l'anniversaire de";

            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, _idChannelGeneral);
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
                    //string message = msgStart + $" <@{id}> aujourd'hui !\n" +
                    string msgTmp = $"@here {_pikachuEmote} \n" +
                        $"On me souffle dans l'oreille que c'est l'anniversaire de"; // TO REMOVE NEXT BD
                    string message = msgTmp + $" <@{id}> aujourd'hui !\n" +
                    $"*PS : J'ai pas vraiment d'oreille*"; // TODO : METTRE coeur à la place

                    if (channel != null)
                    {
                        var res = (IMessage)channel.SendMessageAsync(message).Result;
                        await AddReactionBirthDay(res);
                    }
                    else log.Error("Can't wish HB because general was not found");
                }
            }                        
        }

        internal void OnePieceDispo()
        {
            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, _idJellyfinChannel);                        

            channel.SendMessageAsync(Helper.GetOnePieceMessage(_dlEmoji, _coeurEmote));
        }

        internal void SendToLeader(string message)
        {
            var leader = _client.GetUser(312317884389130241);
            leader.SendMessageAsync(message);            
        }

        #region Control Message
        internal async Task CommandNotAuthorize(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"L'utilisation de cette commande est limitée au channel <#826144013920501790>", messageReference: reference);
        }

        internal async Task SendJellyfinNotAuthorize(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"⚠️ Pour des raisons de sécurité l'utilisation de Jellyfin" +
                $" est limitée au channel <#816283362478129182>", messageReference: reference);
            await channel.SendMessageAsync($"```Contacte Vince pour qu'il te créé un compte```");            
        }

        internal async Task SendJellyfinAlreadyInUse(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync($"{_alarmEmote} Un lien a déjà été généré {_alarmEmote}\n" +
                $"En cas de soucis merci de contacter <@!312317884389130241>");            
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
                ImageUrl = _discordImgUrl,
                ThumbnailUrl = _boToolsGif,

                Title = $"{GetCheckEmote()}︱Cliquez ici︱{GetCheckEmote()}",
                Description = $"{GetCoinEmote()}  Relancer **$Jellyfin** si le lien ne fonctionne plus\n" +
                    $"{GetCoinEmote()}  En cas de problème : **$BUG**",

                Author = new EmbedAuthorBuilder { Name = "Jellyfin requested by " + userMsg.Author.Username, IconUrl = userMsg.Author.GetAvatarUrl() },
                Footer = GetFooterBuilder()
            };
        }
        public EmbedBuilder MakeInternalJellyfinMessageBuilder(string ngRockUrl)
        {
            return new EmbedBuilder
            {
                Url = ngRockUrl,
                Color = Color.DarkRed,
                ImageUrl = _discordImgUrl,
                ThumbnailUrl = _boToolsGif,

                Title = $"{GetCheckEmote()}︱Streaming & Download︱{GetCheckEmote()}",
                Description = $"{GetCoinEmote()}  Relancer **$Jellyfin** si le lien ne fonctionne plus\n" +
                    $"{GetCoinEmote()}  Contacter Vince en cas de problème, je ne suis que de simple lignes de code : je peux pas tout faire !",

                Author = new EmbedAuthorBuilder { Name = "Ngrok c'est quand il veut !", IconUrl = _urlAvatarVince },
                Footer = GetFooterBuilder()
            };
        }

        private EmbedBuilder MakeMessageBuilder(SocketGuildUser guildUser)
        {                        
            EmbedBuilder res = new EmbedBuilder
            {                
                Color = Color.DarkRed,                
                ThumbnailUrl = _boToolsGif,

                Title = $"{GetCheckEmote()}︱WELCOME︱{GetCheckEmote()}",
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
                IconUrl = _urlAvatarVince,
                Text = $"Powered with {GetCoeurEmoji()} by Vince"
            };
        }
        #endregion

        #region Get Emoji/Emote
        public string GetCoinEmote() { return _coinEmote; }
        public string GetCoeurEmote() { return _coeurEmote; }
        public string GetCheckEmote() { return _checkEmote; }
        public string GetCatVibeEmote() { return _catVibeEmote; } 
        public string GetArrowEmote() { return _arrowEmote; }
        public string GetDoneEmote() { return _doneEmote; }
        public string GetPepeSmokeEmote() { return _pepeSmokeEmote; }
        public string GetLuffyEmote() { return _luffyEmote; }
        public string GetCoeurEmoji() { return _coeurEmoji; }
        public string GetTvEmoji() { return _tvEmoji; }
        #endregion
    }
}
