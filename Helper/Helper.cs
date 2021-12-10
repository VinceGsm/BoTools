using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BoTools
{
    public static class Helper
    {
        private static readonly List<string> _greetings = new List<string>
        {
            "good day","salutations","hey","oh les bg !","petites cailles bonjour","ciao a tutti", "insérer une phrase cool",
            "konnichi wa","'sup, b?","what's poppin'?","greetings","What's in the bag?","sup","wussup?","how ya goin?",
            "what's the dizzle?","good morning","what's cracking?","quoi de neuf la cité ?","whazzup?","guten Tag",
            "EDGAAAAAAR","good afternoon","hola","hello","coucou !","what's the dilly?","très heureux d'être là",
            "wassap?","what's the rumpus?","what's crackin'?","how do?","yello","what's up?","c'est moi que revoilà !",
            "on est pas pressé, mais moi oui","what's new?","what's shaking?","howzit?","good night","hola","ahoy",
            "aloha","how's it hanging?","howsyamomanem?","how goes it?","good evening","yo","how's it going?",
            "ça dit quoi les filles ?", "Ah ! Toujours là ce bon vieux Denis","what's cooking?", "invocation"
        };
        public static readonly List<ulong> _rolesAttributionId = new List<ulong>
        {
            698852663764451381, //games
            620700703580618762, //casino
            613331423000133634, //anime
            536174000439558185, //music
            802193968402137148, //contentCreator
            613381032569339943, //stoner
            552134779210825739, //OP
            773174545258774568, //visio
            588340175357214729, //pc
            797968836707352606, //mac
            572073517462323201, //switch
            572076063887327242, //playst
            607260961765589032, //fortnite
            689157917521346684, //mine
            843280439698259998, //bf            
            638175689270493205, //cod
            818518545720803341, //gta
            615822402781315073, //lol
            712589813605203979, //wow
        };


        private static Dictionary<string, DateTime> _birthsDay = new Dictionary<string, DateTime>();
        private static readonly string _zderLandId = Environment.GetEnvironmentVariable("ZderLandId");
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //internal static SocketGuildChannel GetSocketChannel(DiscordSocketClient client, string channelName)
        //{
        //    var channels = GetAllChannels(client);

        //    SocketGuildChannel channel = channels.FirstOrDefault(x => x.Name == channelName);

        //    if (channel == null) log.Error($"GetSocketChannel : no channel {channelName}");

        //    return channel;
        //}

        internal static ISocketMessageChannel GetSocketMessageChannel(DiscordSocketClient client, ulong channelId)
        {
            var channels = GetAllChannels(client);

            ISocketMessageChannel channel = (ISocketMessageChannel)channels.FirstOrDefault(x => x.Id == channelId);

            if (channel == null) log.Error($"GetSocketMessageChannelContains : no channel {channelId}");

            return channel;
        }

        internal static IEnumerable<IRole> GetRolesAttribution(DiscordSocketClient client)
        {
            List<IRole> res = new List<IRole>();

            foreach (ulong id in _rolesAttributionId)
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

        internal static bool IsJellyfinCorrectChannel(ISocketMessageChannel channel)
        {
            return channel.Name.ToLower().Contains("jellyfin");
        }

        internal static SocketGuild GetZderLand(DiscordSocketClient client)
        {
            return client.Guilds.FirstOrDefault(); // in prod the bot is strictly connected to Zderland
            //return client.GetGuild(Convert.ToUInt64(_zderLandId)); //in case of using testServer
        }
        internal static IEnumerable<SocketGuild> GetZderLands(DiscordSocketClient client)
        {
            return client.Guilds;            
        }

        internal static IEnumerable<SocketGuildChannel> GetAllChannels(DiscordSocketClient client)
        {
            SocketGuild guild = GetZderLand(client);
            var channels = guild.Channels.ToList();
            channels.Remove(channels.ElementAt(0)); // conference

            return channels;
        }

        internal static string GetGreeting()
        {
            Random random = new Random();
            string res = _greetings[random.Next(_greetings.Count)];

            //First letter Uper
            return res.First().ToString().ToUpper() + res.Substring(1); 
        }

        internal static string GetOnePieceMessage(string dlEmoji, string coeurEmote)
        {
            var r = new Random();
            int i = r.Next(2);

            string startMsg = $" {dlEmoji} <@&552134779210825739> \n";
            string messageKanji = startMsg +
                "よろしくお願いします\nワンピースの最後のエピソードが利用可能です。次回の視聴のために、" +
                $"事前にダウンロードすることを躊躇しないでください。ありがとう、\nキス {coeurEmote}";
            string messageJap = startMsg +
                "Yoroshikuonegaītashimasu,\nOne Piece no saigo no episōdo ga riyō kanōdesu. " +
                $"Jikai no shichō no tame ni, jizen ni daunrōdo suru koto o chūcho shinaide kudasai.\nArigatō, kisu {coeurEmote}";

            return (i == 0) ? messageJap : messageKanji; // 50% Jap / 50% Kanji
        }

        internal static Dictionary<string,DateTime> GetBirthsDay()
        {
            _birthsDay.Add("!427918309594234881", DateTime.Parse("03/01")); //Coco
            _birthsDay.Add("!312317884389130241", DateTime.Parse("22/01")); //Vince
            _birthsDay.Add("!560259660578291715", DateTime.Parse("14/02")); //Babiss
            _birthsDay.Add("!418426899786760194", DateTime.Parse("21/02")); //Jerem
            _birthsDay.Add("!342944682579460096", DateTime.Parse("16/04")); //Matthieu
            _birthsDay.Add("!126259389962125312", DateTime.Parse("02/06")); //Flo
            _birthsDay.Add("!706958493374218300", DateTime.Parse("03/06")); //Julio
            _birthsDay.Add("!511225222545014817", DateTime.Parse("30/06")); //Isma
            _birthsDay.Add("!391570155458527233", DateTime.Parse("06/07")); //Quentinus
            _birthsDay.Add("!444958143081086986", DateTime.Parse("21/07")); //Dan
            _birthsDay.Add("!418459600959045633", DateTime.Parse("22/07")); //Wbr
            _birthsDay.Add("!421026192523526155", DateTime.Parse("28/07")); //Kiki
            _birthsDay.Add("!293029908761018368", DateTime.Parse("13/08")); //Mathis
            _birthsDay.Add("!270295016797241344", DateTime.Parse("14/09")); //Orel
            _birthsDay.Add("!869869706344034314", DateTime.Parse("27/09")); //Niros
            _birthsDay.Add("!318827498630545418", DateTime.Parse("30/09")); //Louis
            _birthsDay.Add("!696314945725530185", DateTime.Parse("05/10")); //Lena
            _birthsDay.Add("!558802761018376219", DateTime.Parse("24/10")); //Adrizou
            _birthsDay.Add("!409002227575947264", DateTime.Parse("18/11")); //Marwan
            _birthsDay.Add("!709496617895460905", DateTime.Parse("29/11")); //Antonin
            _birthsDay.Add("!270294861490421760", DateTime.Parse("02/12")); //Maxbibi     
            _birthsDay.Add("!312967790619525142", DateTime.Parse("03/12")); //Anto
            _birthsDay.Add("!143706383064367104", DateTime.Parse("04/12")); //Nico            
            _birthsDay.Add("!173837924599726080", DateTime.Parse("09/12")); //Paul
            _birthsDay.Add("!355731040913850398", DateTime.Parse("23/12")); //Majid            
            return _birthsDay;
        }

        internal static bool IsLogChannel(ISocketMessageChannel channel)
        {
            return channel.Name.EndsWith("log");
        }
    }
}
