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
        private static string _eternalInvite = "https://discord.gg/g43kWat";
        #region emote                
        private static readonly string _coinEmote = "<a:Coin:637802593413758978>";
        private static readonly string _doneEmote = "<a:check:626017543340949515>";
        private static readonly string _arrowEmote = "<a:arrow:830799574947463229>";
        private static readonly string _alarmEmote = "<a:alert:637645061764415488>";
        private static readonly string _coeurEmote = "<a:coeur:830788906793828382>";
        private static readonly string _bravoEmote = "<a:bravo:626017180731047977>";
        private static readonly string _checkEmote = "<a:verified:773622374926778380>";        
        private static readonly string _catVibeEmote = "<a:catvibe:792184060054732810>";
        private static readonly string _pikachuEmote = "<a:hiPikachu:637802627345678339>";
        private static readonly string _pepeSmokeEmote = "<a:pepeSmoke:830799658354737178>";               
        #endregion
        #region emoji
        private static readonly string _coeurEmoji = "\u2764";
        private static readonly string _tvEmoji = "\uD83D\uDCFA";
        #endregion
        private DiscordSocketClient _client;
        private ISocketMessageChannel _logChannel;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public MessageService(DiscordSocketClient client)
        {
            _client = client;                                   
            _client.Ready += Ready;            
            _client.UserLeft += UserLeft;
            _client.InviteCreated += InviteCreated;            
        }

        #region Client
        /// <summary>
        /// When guild data has finished downloading (+state : Ready)
        /// </summary>
        /// <returns></returns>
        public async Task Ready()
        {
            _logChannel = Helper.GetSocketMessageChannel(_client, "log");

            await SendLatencyAsync();                        
            await CheckBirthday();

            return;
        }

        /// <summary>
        /// When a User left the Guild
        /// </summary>
        /// <param name="guildUser"></param>
        /// <returns></returns>
        private async Task UserLeft(SocketGuildUser guildUser)
        {
            string user = guildUser.Username + '#' + guildUser.Discriminator;
            string joinedAt = Helper.ConvertToSimpleDate(guildUser.JoinedAt.Value);                                    
            string message = $"```{user} left Zderland ! This person joined at {joinedAt}```";             

            if (_logChannel != null)
                await _logChannel.SendMessageAsync(message);

            return;
        }

        private Task InviteCreated(SocketInvite invite)
        {                        
            var channel = Helper.GetSocketMessageChannel(_client, invite.Channel.Name);

            string duration = (invite.IsTemporary) ? "éternelle" : $"valable {invite.MaxAge/3600}h";

            string logMessage = $"Une nouvelle invitation *{duration}* à Zderland vient d'être créée par " +
                $"**{invite.Inviter.Username}** dans : {channel?.Name}";

            string message = $"{_alarmEmote} Voici l'invitation officielle à partager por favor : {_eternalInvite} {_coeurEmote}";

            if (_logChannel != null)            
                _logChannel.SendMessageAsync(logMessage);
                
            if (channel != null)
                channel.SendMessageAsync(message);        

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
            string message = $"{Helper.GetGreeting()}```Je suis à {_client.Latency}ms de Zderland !```";

            if (_logChannel != null)            
                await _logChannel.SendMessageAsync(message, isTTS:true);
            
            log.Info($"Latency : {_client.Latency} ms");
        }

        private async Task CheckBirthday()
        {
            bool isAlreadyDone = false;
            string msgStart = $"@everyone {_pikachuEmote} \n" +
                        $"On me souffle dans l'oreille que c'est l'anniversaire de";

            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, "general");
            var msg = channel.GetMessagesAsync(50).ToListAsync().Result;
            
            foreach (var list in msg)
            {
                IEnumerable<IMessage> messages = list.Where(x => x.Content.StartsWith(msgStart) && x.Timestamp.DayOfYear == DateTimeOffset.Now.DayOfYear);
         
                isAlreadyDone = messages.Any();
            }

            if (!isAlreadyDone)
            {
                Dictionary<string, DateTime> birthsDay = Helper.GetBirthsDay();
                bool isSomeoneBD = birthsDay.ContainsValue(DateTime.Today);

                if (isSomeoneBD)
                {
                    string id = birthsDay.First(x => x.Value == DateTime.Today).Key;
                    string message = msgStart + $" <@{id}> aujourd'hui !\n" +
                        $"*ps : j'ai pas vraiment d'oreille*";                    

                    if (channel != null)
                    {
                        var res = (IMessage)channel.SendMessageAsync(message).Result;
                        await AddReactionBirthDay(res);
                    }
                }
            }
            
            return;
        }

        public async Task SendJellyfinNotAuthorize(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync($"⚠️ Pour des raisons de sécurité l'utilisation de Jellyfin" +
                $" est limité au channel 🌐︱jellyfin ⚠️");
            await channel.SendMessageAsync($"```Contacte Vince pour qu'il te créé un compte```<#816283362478129182>");            
            return;
        }

        public async Task SendJellyfinAlreadyInUse(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync($"{_alarmEmote} Un lien a déjà été généré {_alarmEmote}\n" +
                $"En cas de soucis merci de contacter <@!312317884389130241>");
            return;
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

        public string GetCoeurEmoji() { return _coeurEmoji; }
        public string GetTvEmoji() { return _tvEmoji; }
        #endregion

        private static bool IsStaffMsg(SocketMessage msg)
        {            
            return (msg.Author.IsBot || msg.Author.Username.StartsWith("Vince"));
        }
    }
}
