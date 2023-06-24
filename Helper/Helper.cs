using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools
{
    public static class Helper
    {
        public static readonly string statusLink = "https://www.twitch.tv/vince_zder";
        public static readonly string _zderLandIconUrl = "https://cdn.discordapp.com/attachments/494958624922271745/1056847373436977162/brookByVince.gif";        
        public static readonly string _boToolsGif = "https://cdn.discordapp.com/attachments/553256709439750151/1062431704914067566/KatakuriLow.gif";
        public static readonly string _urlAvatarVince = "https://cdn.discordapp.com/attachments/617462663374438411/846821971114983474/luffy.gif";
        public static readonly string _urlQuestionGif = "https://cdn.discordapp.com/attachments/617462663374438411/1122152112399339581/question.gif";
        public static readonly string _urlListGif = "https://cdn.discordapp.com/attachments/617462663374438411/1122165956983132250/list.gif";
        public static readonly ulong _ZderLandId = 312966999414145034;        
        public static readonly ulong _idModoRole = 322489502562123778;
        public static readonly ulong _idGeneralChannel = 312966999414145034;
        public static readonly ulong _idJellyfinChannel = 816283362478129182;        
        public static readonly ulong _idOnePieceChannel = 553256709439750151;
        public static readonly ulong _idSaloonVoice = 493036345686622210;
        private static readonly ulong _idModoChannel = 539151743213240331;
                  
        private static readonly string _coinEmote = "<a:Coin:637802593413758978>";
        private static readonly string _doneEmote = "<a:check:626017543340949515>";
        private static readonly string _arrowEmote = "<a:arrow:830799574947463229>";
        private static readonly string _alarmEmote = "<a:alert:637645061764415488>";
        private static readonly string _coeurEmote = "<a:coeur:830788906793828382>";
        private static readonly string _bravoEmote = "<a:bravo:626017180731047977>";
        private static readonly string _luffyEmote = "<a:luffy:863101041498259457>";
        private static readonly string _verifiedEmote = "<a:verified:773622374926778380>";                
        private static readonly string _pikachuEmote = "<a:hiPikachu:637802627345678339>";
        private static readonly string _pepeSmokeEmote = "<a:pepeSmoke:830799658354737178>";  
        private static readonly string _pepeMdrEmote = "<a:pepeMDR:912738745105674292>";
        private static readonly string _heheEmote = "<a:hehe:773622227064979547>";
        private static readonly string _coeurEmoji = "\u2764";        
        private static readonly string _tvEmoji = "\uD83D\uDCFA";
        private static readonly string _dlEmoji = "<:DL:894171464167747604>";

        private static readonly List<string> _greetings = new List<string>
        {
            "good day","salutations","hey","oh les bg !","petites cailles bonjour","ciao a tutti",
            "konnichi wa","'sup, b?","what's poppin'?","greetings","What's in the bag?","sup","wussup?","how ya goin?",
            "good morning","what's cracking?","quoi de 9 la 6 thé ?","whazzup?","guten Tag",
            "EDGAAAAAAR","good afternoon","hola","hello","coucou !","what's the dilly?","très heureux d'être là",
            "wassap?","what's crackin'?","how do?","yello","what's up?","c'est moi que revoilà !",
            "on est pas pressé, mais moi oui","what's new?","what's shaking?","howzit?","good night","hola","ahoy",
            "aloha","how's it hanging?","howsyamomanem?","how goes it?","good evening","yo","how's it going?",
            "ça dit quoi les filles ?", "Ah ! Toujours là ce bon vieux Denis","what's cooking?", "invocation !"
        };

        private static Dictionary<string, DateTime> _birthsDay = new Dictionary<string, DateTime>();
        private static readonly string _zderLandId = Environment.GetEnvironmentVariable("ZderLandId");
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        

        internal static ISocketMessageChannel GetSocketMessageChannel(DiscordSocketClient client, ulong channelId)
        {
            var channels = GetAllChannels(client);

            ISocketMessageChannel channel = (ISocketMessageChannel)channels.FirstOrDefault(x => x.Id == channelId);

            if (channel == null) log.Error($"GetSocketMessageChannelContains : no channel {channelId}");

            return channel;
        }

        internal static ISocketMessageChannel GetSocketMessageChannelModo(DiscordSocketClient client)
        {
            var channels = GetAllChannels(client);

            ISocketMessageChannel channel = (ISocketMessageChannel)channels.FirstOrDefault(x => x.Id == _idModoChannel);

            if (channel == null) log.Error($"GetSocketMessageChannelContains : no channel LogChannel");

            return channel;
        }

        internal static IEnumerable<IRole> GetIRolesFromServer(DiscordSocketClient client, List<ulong> rolesId)
        {
            List<IRole> res = new List<IRole>();

            foreach (ulong id in rolesId)
            {
                res.Add(GetZderLand(client).GetRole(id));
            }

            return res;
        }

        internal static IRole GetRoleById(DiscordSocketClient client, ulong id)
        {            
            return GetZderLand(client).Roles.First(x => x.Id == id);            
        }

        internal static string ConvertToSimpleDate(DateTimeOffset dateTimeOffset)
        {
            DateTime joinedDate = DateTime.Parse(dateTimeOffset.ToString());
            return joinedDate.Date.ToString().Replace("00:00:00", joinedDate.TimeOfDay.ToString());
        }

        /// <summary>
        /// Return SocketGuild as ZderLand
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        internal static SocketGuild GetZderLand(DiscordSocketClient client)
        {
            return client.Guilds.FirstOrDefault(); // in prod the bot is strictly connected to Zderland            
        }

        internal static IEnumerable<SocketGuildChannel> GetAllChannels(DiscordSocketClient client)
        {
            SocketGuild guild = GetZderLand(client);
            var channels = guild.Channels.ToList();            

            return channels;
        }

        internal static List<IThreadChannel> GetAllActiveThread(ITextChannel channel)
        {
            return channel.GetActiveThreadsAsync().Result.ToList();
        }

        internal static Task ClosedAllActiveThread(ITextChannel channel)
        {
            var threads = GetAllActiveThread(channel);
            int cpt = threads.Count;
            foreach (var thread in threads) { thread.ModifyAsync(x => x.Archived = true).Wait(); }
            log.Info($"ClosedAllThread done for {cpt} in {channel.Name}");
            return Task.CompletedTask;
        }

        #region Message
        internal static string GetGreeting()
        {
            Random random = new Random();
            string res = _greetings[random.Next(_greetings.Count)];

            //First letter Uper
            return res.First().ToString().ToUpper() + res.Substring(1);
        }
        #endregion

        internal static bool IsSundayToday() { return DateTime.Now.DayOfWeek == DayOfWeek.Sunday; }
        internal static bool IsFridayToday() { return DateTime.Now.DayOfWeek == DayOfWeek.Friday; }

        internal static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            start = start.AddDays(1); //excluding today if its corresponding to 'day'
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        internal static Dictionary<string,DateTime> GetBirthDays()
        {
            log.Info("GetBirthDays call v2");

            CultureInfo culture = new CultureInfo("fr-FR");
            _birthsDay.Add("!786748190283792414", DateTime.ParseExact("03/01", "dd/MM", culture)); //Coco            
            _birthsDay.Add("!560259660578291715", DateTime.ParseExact("14/02", "dd/MM", culture)); //Babiss
            //_birthsDay.Add("!418426899786760194", DateTime.ParseExact("21/02", "dd/MM", culture)); //Jerem            
            _birthsDay.Add("!126259389962125312", DateTime.ParseExact("02/06", "dd/MM", culture)); //Flo
            //_birthsDay.Add("!706958493374218300", DateTime.ParseExact("03/06", "dd/MM", culture)); //Julio
            _birthsDay.Add("!511225222545014817", DateTime.ParseExact("30/06", "dd/MM", culture)); //Isma
            _birthsDay.Add("!391570155458527233", DateTime.ParseExact("06/07", "dd/MM", culture)); //Quentinus
            //_birthsDay.Add("!444958143081086986", DateTime.ParseExact("21/07", "dd/MM", culture)); //Dan
            _birthsDay.Add("!418459600959045633", DateTime.ParseExact("22/07", "dd/MM", culture)); //Wbr
            _birthsDay.Add("!421026192523526155", DateTime.ParseExact("28/07", "dd/MM", culture)); //Kiki
            _birthsDay.Add("!293029908761018368", DateTime.ParseExact("13/08", "dd/MM", culture)); //Mathis
            _birthsDay.Add("!270295016797241344", DateTime.ParseExact("14/09", "dd/MM", culture)); //Orel
            _birthsDay.Add("!869869706344034314", DateTime.ParseExact("27/09", "dd/MM", culture)); //Niros
            _birthsDay.Add("!318827498630545418", DateTime.ParseExact("30/09", "dd/MM", culture)); //Louis
            _birthsDay.Add("!696314945725530185", DateTime.ParseExact("05/10", "dd/MM", culture)); //Lena
            _birthsDay.Add("!558802761018376219", DateTime.ParseExact("24/10", "dd/MM", culture)); //Adrizou
            _birthsDay.Add("!409002227575947264", DateTime.ParseExact("18/11", "dd/MM", culture)); //Marwan
            //_birthsDay.Add("!709496617895460905", DateTime.ParseExact("29/11", "dd/MM", culture)); //Antonin
            _birthsDay.Add("!270294861490421760", DateTime.ParseExact("02/12", "dd/MM", culture)); //Maxbibi     
            _birthsDay.Add("!312967790619525142", DateTime.ParseExact("03/12", "dd/MM", culture)); //Anto
            _birthsDay.Add("!143706383064367104", DateTime.ParseExact("04/12", "dd/MM", culture)); //Nico            
            _birthsDay.Add("!173837924599726080", DateTime.ParseExact("09/12", "dd/MM", culture)); //Paul
            _birthsDay.Add("!355731040913850398", DateTime.ParseExact("23/12", "dd/MM", culture)); //Majid            
            return _birthsDay;            
        }


        #region Get Emoji/Emote
        public static string GetCoinEmote() { return _coinEmote; }
        public static string GetCoeurEmote() { return _coeurEmote; }
        public static string GetVerifiedEmote() { return _verifiedEmote; }
        public static string GetPikachuEmote() { return _pikachuEmote; }
        public static string GetAlarmEmote() { return _alarmEmote; }
        public static string GetBravoEmote() { return _bravoEmote; }        
        public static string GetArrowEmote() { return _arrowEmote; }
        public static string GetDoneEmote() { return _doneEmote; }
        public static string GetPepeSmokeEmote() { return _pepeSmokeEmote; }
        public static string GetPepeMdrEmote() { return _pepeMdrEmote; }
        public static string GetHeheEmote() { return _heheEmote; } 
        public static string GetLuffyEmote() { return _luffyEmote; }
        public static string GetCoeurEmoji() { return _coeurEmoji; }
        public static string GetTvEmoji() { return _tvEmoji; }
        public static string GetDlEmoji() { return _dlEmoji; }

        internal static string GetZderLandIconUrl()
        {
            return _zderLandIconUrl;
        }
        internal static string GetZderLandId()
        {
            return _zderLandId;
        }
        #endregion
    }
}
