using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class EventService
    {
        private DiscordSocketClient _client;
        private SocketGuild _serv;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EventService(DiscordSocketClient client)
        {
            _client = client;
            _serv = Helper.GetZderLand(_client);
        }

        public async Task CreateNextOnePiece(bool isJellyfinRequest = false)
        {            
            var events = _serv.GetEventsAsync().Result.ToList();

            bool isNeeded = events.Where(x => x.Name.StartsWith("One Piece 1")).Count() < 1 ;

            if (isNeeded || isJellyfinRequest)
            {
                var name = $"One Piece {GetNextNumOnePiece()} Streaming";
                DateTime target = Helper.GetNextWeekday(DateTime.Today, DayOfWeek.Sunday);
                DateTimeOffset startTime = new DateTimeOffset(target.AddHours(21.2));   // 21h12
                GuildScheduledEventType type = GuildScheduledEventType.Voice;
                string description = "**RDV hebdomadaire du server !**";
                ulong? channelId = Helper._idSaloonVoice;
                Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "Onepiece.png"));

                _serv.CreateEventAsync(name, startTime: startTime, type: type, description: description, channelId: channelId, coverImage: coverImage);
            }
            else
                log.Warn("CreateNextOnePiece : An event is already programmed !");
        }

        public int GetNextNumOnePiece()
        {            
            string html;                     
            var htmlDoc = new HtmlDocument();

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    html = httpClient.GetStringAsync("https://www.vostfr-episode.com/anime-one-piece").Result;
                }
                htmlDoc.LoadHtml(html);

                string xpathBeforeTarget = "/html[1]/body[1]/div[2]/div[4]/div[6]/div[1]/div[1]/ul[1]/li[21]/a[1]";
                var listNode = htmlDoc.DocumentNode.Descendants("a").ToList();
                var beforeTargetNode = htmlDoc.DocumentNode.SelectSingleNode(xpathBeforeTarget);

                var target = listNode[listNode.IndexOf(beforeTargetNode) + 1].InnerText.Trim();

                var season = Regex.Match(target, @"\d+").Value;
                int num = Int32.Parse(target.Substring(13, 4));

                return num + 1;
            }
            catch(Exception ex) { return 0; }
        }

        internal void CreateEventSeries(string name, int numFirstEpisode, int nbEp, string dayOfWeek, Double hour)
        {
            log.Info("CreateEventSeries IN");

            DayOfWeek targetDayOfWeek;            
            switch (dayOfWeek)
            {
                case "lundi":
                case "Lundi":
                case "monday":
                case "Monday":
                    targetDayOfWeek = DayOfWeek.Monday;
                    break;
                case "mardi":
                case "Mardi":
                case "tuesday":
                case "Tuesday":
                    targetDayOfWeek = DayOfWeek.Tuesday;
                    break;
                case "mercredi":
                case "Mercredi":
                case "wednesday":
                case "Wednesday":
                    targetDayOfWeek = DayOfWeek.Wednesday;
                    break;
                case "jeudi":
                case "Jeudi":
                case "thursday":
                case "Thursday":
                    targetDayOfWeek = DayOfWeek.Thursday;
                    break;
                case "vendredi":
                case "Vendredi":
                case "friday":
                case "Friday":
                    targetDayOfWeek = DayOfWeek.Friday;
                    break;
                case "samedi":
                case "Samedi":
                case "saturday":
                case "Saturday":
                    targetDayOfWeek = DayOfWeek.Saturday;
                    break;
                case "dimanche":
                case "Dimanche":
                case "sunday":
                case "Sunday":
                    targetDayOfWeek = DayOfWeek.Sunday;
                    break;

                default:
                    targetDayOfWeek = DayOfWeek.Thursday;
                    break;
            }

            DateTime target = DateTime.Now;
            DateTime firstTargetDay = Helper.GetNextWeekday(DateTime.Today, targetDayOfWeek);

            for (int i=0; i<nbEp; i++)
            {
                var nameEvent = $"{name} #{numFirstEpisode} Streaming";
                log.Info($"CreateEventSeries : {nameEvent}");
                
                if (i == 0)
                    target = firstTargetDay;
                else                
                    target = Helper.GetNextWeekday(target, targetDayOfWeek);
                                
                DateTimeOffset startTime = new DateTimeOffset(target.AddHours(hour));
                GuildScheduledEventType type = GuildScheduledEventType.Voice;
                string description = "À voir ou à télécharger sur Jellyfin !";
                ulong? channelId = Helper._idSaloonVoice;
                Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "eventDiscord.png"));

                _serv.CreateEventAsync(nameEvent, startTime: startTime, type: type, description: description, channelId: channelId, coverImage: coverImage);

                numFirstEpisode++;
            }

            log.Info("CreateEventSeries OUT");            
        }
    }
}
