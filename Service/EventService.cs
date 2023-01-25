using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class EventService
    {
        private DiscordSocketClient _client;        
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EventService(DiscordSocketClient client)
        {
            _client = client;            
        }

        public async Task CreateNextOnePiece(bool isJellyfinRequest = false)
        {
            SocketGuild _serv = Helper.GetZderLand(_client);
            var events = _serv.GetEventsAsync().Result.ToList();

            var name = $"One Piece {GetNextNumOnePiece()} Streaming";
            DateTime target = Helper.GetNextWeekday(DateTime.Today, DayOfWeek.Sunday);
            DateTimeOffset startTime = new DateTimeOffset(target.AddHours(21));   // 21h
            GuildScheduledEventType type = GuildScheduledEventType.Voice;
            string description = "**RDV hebdomadaire du server !**";
            ulong? channelId = Helper._idSaloonVoice;
            Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "Onepiece.png"));

            _serv.CreateEventAsync(name, startTime: startTime, type: type, description: description, channelId: channelId, coverImage: coverImage);
        }

        public int GetNextNumOnePiece()
        {            
            string html;                     
            var htmlDoc = new HtmlDocument();

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    html = httpClient.GetStringAsync("https://www.themoviedb.org/tv/37854/episode_group/5ad0f096c3a36825a300e78b/group/5ad0f3230e0a260942006562").Result;
                }
                htmlDoc.LoadHtml(html);
                
                var nbEpReleased = htmlDoc.DocumentNode.SelectNodes("//span[contains(@class, 'episode_number')]").Count;
                
                return nbEpReleased + 2; //check if regular
            }
            catch(Exception ex) 
            { log.Error(ex.Message); return 0; }
        }

        public async void CreateEventSeries(string name, int numFirstEpisode, int nbEp, DayOfWeek dayOfWeek, Double hour)
        {
            log.Info("CreateEventSeries IN");

            SocketGuild _serv = Helper.GetZderLand(_client);

            DateTime target = DateTime.Now;
            DateTime firstTargetDay = Helper.GetNextWeekday(DateTime.Today, dayOfWeek);
            
            for (int i=0; i<nbEp; i++)
            {
                var nameEvent = $"{name} #{numFirstEpisode} Streaming";
                log.Info($"CreateEventSeries : {nameEvent}");

                try
                {
                    if (i == 0)
                        target = firstTargetDay;
                    else
                        target = Helper.GetNextWeekday(target, dayOfWeek);

                    DateTimeOffset startTime = new DateTimeOffset(target.AddHours(hour));
                    GuildScheduledEventType type = GuildScheduledEventType.Voice;
                    string description = "À voir ou à télécharger sur Jellyfin !";
                    ulong? channelId = Helper._idSaloonVoice;
                    Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "eventDiscord.png"));

                    _serv.CreateEventAsync(nameEvent, startTime: startTime, type: type, description: description, channelId: channelId, coverImage: coverImage);
                }
                catch(Exception ex)
                {
                    log.Error(ex.InnerException.Message);
                }

                numFirstEpisode++;                
            }

            log.Info("CreateEventSeries OUT");            
        }

        public async void CreateEventSeries(string name, int numFirstEpisode, int nbEp, DayOfWeek dayOfWeek, Double hour)
        {
            log.Info("CreateEventSeries IN");

            SocketGuild _serv = Helper.GetZderLand(_client);

            DateTime target = DateTime.Now;
            DateTime firstTargetDay = Helper.GetNextWeekday(DateTime.Today, dayOfWeek);
            
            for (int i=0; i<nbEp; i++)
            {
                var nameEvent = $"{name} #{numFirstEpisode} Streaming";
                log.Info($"CreateEventSeries : {nameEvent}");

                try
                {
                    if (i == 0)
                        target = firstTargetDay;
                    else
                        target = Helper.GetNextWeekday(target, dayOfWeek);

                    DateTimeOffset startTime = new DateTimeOffset(target.AddHours(hour));
                    GuildScheduledEventType type = GuildScheduledEventType.Voice;
                    string description = "À voir ou à télécharger sur Jellyfin !";
                    ulong? channelId = Helper._idSaloonVoice;
                    Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "eventDiscord.png"));

                    _serv.CreateEventAsync(nameEvent, startTime: startTime, type: type, description: description, channelId: channelId, coverImage: coverImage);
                }
                catch(Exception ex)
                {
                    log.Error(ex.InnerException.Message);
                }

                numFirstEpisode++;                
            }

            log.Info("CreateEventSeries OUT");            
        }
    }
}
