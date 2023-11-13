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
        public static readonly ulong _vinceId = 312317884389130241;
        public static readonly ulong _vinceBisId = 493020872303443969;
        public static readonly ulong _squadVoiceId = 1007423970670297178;
        public static readonly ulong _vocalCategoryId = 493018545089806337;
        public static readonly ulong _idModoRole = 322489502562123778;
        public static readonly ulong _idGeneralChannel = 312966999414145034; 
        public static readonly ulong _idJellyfinChannel = 816283362478129182;        
        public static readonly ulong _idOnePieceChannel = 553256709439750151;
        public static readonly ulong _idSaloonVoice = 493036345686622210;
        public static readonly ulong _idModoChannel = 539151743213240331;
        public static readonly ulong _idThreadMeteo = 1171768369926651905;

        public static readonly string _coinEmote = "<a:Coin:637802593413758978>";
        public static readonly string _doneEmote = "<a:check:626017543340949515>";
        public static readonly string _arrowEmote = "<a:arrow:830799574947463229>";
        public static readonly string _alarmEmote = "<a:alert:637645061764415488>";
        public static readonly string _coeurEmote = "<a:coeur:830788906793828382>";
        public static readonly string _bravoEmote = "<a:bravo:626017180731047977>";
        public static readonly string _luffyEmote = "<a:luffy:863101041498259457>";
        public static readonly string _verifiedEmote = "<a:verified:773622374926778380>";                
        public static readonly string _pikachuEmote = "<a:hiPikachu:637802627345678339>";
        public static readonly string _pepeSmokeEmote = "<a:pepeSmoke:830799658354737178>";  
        public static readonly string _pepeMdrEmote = "<a:pepeMDR:912738745105674292>";
        public static readonly string _heheEmote = "<a:hehe:773622227064979547>";
        public static readonly string _coeurEmoji = "\u2764";        
        public static readonly string _tvEmoji = "\uD83D\uDCFA";
        public static readonly string _dlEmoji = "<:DL:894171464167747604>";

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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly string _zderLandId = Environment.GetEnvironmentVariable("ZderLandId");

        private static Dictionary<ulong, string> _tokensOpenAI = new Dictionary<ulong, string>();
        private static readonly string _tokenVinceOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Vince");
        private static readonly string _tokenAntoOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Anto");
        private static readonly string _tokenOrelOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Orel");
        private static readonly string _tokenAdriOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Adri");
        private static readonly string _tokenCocoOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Coco");
        private static readonly string _tokenFloOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Flo");
        private static readonly string _tokenIsmaOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Isma");
        private static readonly string _tokenMaxOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Max");
        private static readonly string _tokenOmarowOpenAI = Environment.GetEnvironmentVariable("OpenAI_Token_Omarow");


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
            //_birthsDay.Add("!560259660578291715", DateTime.ParseExact("14/02", "dd/MM", culture)); //Babiss
            //_birthsDay.Add("!418426899786760194", DateTime.ParseExact("21/02", "dd/MM", culture)); //Jerem            
            _birthsDay.Add("!126259389962125312", DateTime.ParseExact("02/06", "dd/MM", culture)); //Flo
            //_birthsDay.Add("!706958493374218300", DateTime.ParseExact("03/06", "dd/MM", culture)); //Julio
            _birthsDay.Add("!511225222545014817", DateTime.ParseExact("30/06", "dd/MM", culture)); //Isma
            _birthsDay.Add("!391570155458527233", DateTime.ParseExact("06/07", "dd/MM", culture)); //Quentinus
            //_birthsDay.Add("!444958143081086986", DateTime.ParseExact("21/07", "dd/MM", culture)); //Dan
            //_birthsDay.Add("!418459600959045633", DateTime.ParseExact("22/07", "dd/MM", culture)); //Wbr
            //_birthsDay.Add("!421026192523526155", DateTime.ParseExact("28/07", "dd/MM", culture)); //Kiki
            //_birthsDay.Add("!293029908761018368", DateTime.ParseExact("13/08", "dd/MM", culture)); //Mathis
            _birthsDay.Add("!270295016797241344", DateTime.ParseExact("14/09", "dd/MM", culture)); //Orel
            _birthsDay.Add("!869869706344034314", DateTime.ParseExact("27/09", "dd/MM", culture)); //Niros
            _birthsDay.Add("!318827498630545418", DateTime.ParseExact("30/09", "dd/MM", culture)); //Louis
            //_birthsDay.Add("!696314945725530185", DateTime.ParseExact("05/10", "dd/MM", culture)); //Lena
            _birthsDay.Add("!558802761018376219", DateTime.ParseExact("24/10", "dd/MM", culture)); //Adrizou
            //_birthsDay.Add("!409002227575947264", DateTime.ParseExact("18/11", "dd/MM", culture)); //Marwan
            //_birthsDay.Add("!709496617895460905", DateTime.ParseExact("29/11", "dd/MM", culture)); //Antonin
            _birthsDay.Add("!270294861490421760", DateTime.ParseExact("02/12", "dd/MM", culture)); //Maxbibi     
            _birthsDay.Add("!312967790619525142", DateTime.ParseExact("03/12", "dd/MM", culture)); //Anto
            //_birthsDay.Add("!143706383064367104", DateTime.ParseExact("04/12", "dd/MM", culture)); //Nico            
            _birthsDay.Add("!173837924599726080", DateTime.ParseExact("09/12", "dd/MM", culture)); //Paul
            //_birthsDay.Add("!355731040913850398", DateTime.ParseExact("23/12", "dd/MM", culture)); //Majid            
            return _birthsDay;            
        }

        internal static List<string> GetAntoGifUrls()
        {
            return new List<string>
            {
                "https://media.discordapp.net/attachments/1019335397484011581/1095804934353584201/20230412_221638.gif",
                "https://media.discordapp.net/attachments/1019335397484011581/1095801772540428328/20230412_220439.gif",
                "https://media.discordapp.net/attachments/1019335397484011581/1034910422173749328/20221026_212417.gif",
                "https://media.discordapp.net/attachments/713695594878599249/826728090385514526/20210330_163927_1.gif",
                "https://media.discordapp.net/attachments/713695594878599249/827127343256043540/5401fx.gif",
                "https://media.discordapp.net/attachments/784347374071578655/784378508453216266/received_1277813539285202_1.gif",
                "https://media.discordapp.net/attachments/784347374071578655/784368520892645376/IMG_1272.gif",
                "https://media.discordapp.net/attachments/784347374071578655/784380449883422720/received_1303873473299495_4.gif",
                "https://media.discordapp.net/attachments/956285058824683620/971544748479373322/IMG_6609.gif",
                "https://media.discordapp.net/attachments/713695594878599249/738404249037439036/received_239754997016005_1.gif",
                "https://media.discordapp.net/attachments/956285058824683620/1019000369466052698/IMG_9268.gif",
                "https://media.discordapp.net/attachments/956285058824683620/1017185311073239091/20220907_233052.gif",
                "https://media.discordapp.net/attachments/713695594878599249/738400012588941392/received_745278482913700_2.gif",
                "https://media.discordapp.net/attachments/976598205095616562/996137104570589184/11.gif",
                "https://media.discordapp.net/attachments/976598205095616562/996137038355103804/9.gif",
                "https://media.discordapp.net/attachments/976598205095616562/996137288910250075/5.gif",
                "https://media.discordapp.net/attachments/956285058824683620/986717620432699392/IMG_7426.gif",
                "https://media.discordapp.net/attachments/713695594878599249/849173897587720212/video0_2.gif",
                "https://media.discordapp.net/attachments/956285058824683620/986764370635817000/20220616_004904.gif",
                "https://media.discordapp.net/attachments/713695594878599249/738430100365901854/VID-20200730-WA0000_3.gif",
                "https://media.discordapp.net/attachments/713695594878599249/833638139922743327/174350169_287685479580653_8106636962674288332_n.gif",
                "https://media.discordapp.net/attachments/713695594878599249/842071263937691648/20210422_151431_1.gif"
            };
        }


        internal static string GetAntoGifUrl()
        {
            List<string> list = GetAntoGifUrls();

            Random random = new Random();
            return list[random.Next(list.Count)];
        }


        internal static string GetOpenAIToken(ulong idUser)
        {
            if (_tokensOpenAI.Count == 0)
                FillDicoTokens();

            _tokensOpenAI.TryGetValue(idUser, out var token);

            return token;
        }

        private static void FillDicoTokens()
        {
            _tokensOpenAI.Add(_vinceId, _tokenVinceOpenAI);
            _tokensOpenAI.Add(_vinceBisId, _tokenVinceOpenAI);
            _tokensOpenAI.Add(312967790619525142, _tokenAntoOpenAI);
            _tokensOpenAI.Add(270295016797241344, _tokenOrelOpenAI);
            _tokensOpenAI.Add(558802761018376219, _tokenAdriOpenAI);
            _tokensOpenAI.Add(786748190283792414, _tokenCocoOpenAI);
            _tokensOpenAI.Add(126259389962125312, _tokenFloOpenAI);
            _tokensOpenAI.Add(511225222545014817, _tokenIsmaOpenAI);
            _tokensOpenAI.Add(270294861490421760, _tokenMaxOpenAI);
            _tokensOpenAI.Add(318827498630545418, _tokenOmarowOpenAI);
        }
    }
}
