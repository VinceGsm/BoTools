using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using log4net;
using System;
using System.Collections.Generic;
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
                    //html = httpClient.GetStringAsync("https://www.imdb.com/title/tt0388629/episodes/?year=2023").Result;
                    html = httpClient.GetStringAsync("https://www.imdb.com/title/tt0388629/episodes").Result;
                }
                htmlDoc.LoadHtml(html);
                
                var lastnode = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'zero-z-index')]").ToList().Last();
                
                int.TryParse(lastnode.InnerText.Substring(8, 4),out int res);

                return res +1;                
            }
            catch(Exception ex) 
            { log.Error(ex.Message); return 0; }
        }

        public async void CreateEventHebdoSerie(string name, int numFirstEpisode, int nbEp, DayOfWeek dayOfWeek, Double hour)
        {
            log.Info("CreateEventHebdoSerie IN");

            SocketGuild _serv = Helper.GetZderLand(_client);

            DateTime target = DateTime.Now;
            DateTime firstTargetDay = Helper.GetNextWeekday(DateTime.Today, dayOfWeek);
            
            for (int i=0; i<nbEp; i++)
            {
                var nameEvent = $"{name} #{numFirstEpisode} Streaming";
                log.Info($"CreateEventHebdoSerie : {nameEvent}");

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
                    Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "eventSerie.png"));

                    _serv.CreateEventAsync(nameEvent, startTime: startTime, type: type, description: description, channelId: channelId, coverImage: coverImage);
                }
                catch(Exception ex)
                {
                    log.Error(ex.InnerException.Message);
                }

                numFirstEpisode++;                
            }

            log.Info("CreateEventHebdoSerie OUT");            
        }

        public async void CreateEventEnSerie(string name, double hour, int nbSession, bool isIrlEvent,
            DayOfWeek? siLundi, DayOfWeek? siMardi, DayOfWeek? siMercredi, DayOfWeek? siJeudi, DayOfWeek? siVendredi, DayOfWeek? siSamedi, DayOfWeek? siDimanche)
        {
            log.Info("CreateEventEnSerie IN");

            SocketGuild _serv = Helper.GetZderLand(_client);            
            List<DayOfWeek> lstDay = BuildOrderedEventDays(siLundi, siMardi, siMercredi, siJeudi, siVendredi, siSamedi, siDimanche);
            
            DateTime target = DateTime.Today;
            int cptWeek = 0;
            
            for (int i = 0; i < nbSession; i++)
            {                
                var nameEvent = $"{name} #{i + 1}";
                log.Info($"CreateEventEnSerie : {nameEvent}");

                Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "eventEnSerie.png"));
                try
                {                    
                    target = Helper.GetNextWeekday(target, lstDay[cptWeek]);

                    DateTimeOffset startTime = new DateTimeOffset(target.AddHours(hour));                                                                                                    
                    if (isIrlEvent)
                    {
                        GuildScheduledEventType type = GuildScheduledEventType.External;
                        var endTime = startTime.AddHours(2);
                        string description = "Event qui fait prendre l'air, c'est bon ça !";                        

                        _serv.CreateEventAsync(nameEvent, startTime: startTime, endTime: endTime, type: type, description: description, coverImage: coverImage);
                    }
                    else
                    {
                        GuildScheduledEventType type = GuildScheduledEventType.Voice;
                        ulong? channelId = Helper._idSaloonVoice;
                        string description = "Faites de la place dans le Saloon, on va tout péter !!!!";
                        
                        _serv.CreateEventAsync(nameEvent, startTime: startTime, type: type, description: description, channelId: channelId, coverImage: coverImage);
                    }
                    if (cptWeek == lstDay.Count-1) //end of list
                        cptWeek = 0;
                    else
                        cptWeek++;
                }
                catch (Exception ex)
                {
                    log.Error(ex.InnerException.Message);                    
                }
            }
            log.Info("CreateEventEnSerie OUT");
        }

        private static List<DayOfWeek> BuildOrderedEventDays
            (DayOfWeek? siLundi, DayOfWeek? siMardi, DayOfWeek? siMercredi, DayOfWeek? siJeudi, DayOfWeek? siVendredi, DayOfWeek? siSamedi, DayOfWeek? siDimanche)
        {
            List<DayOfWeek> lstDay = new List<DayOfWeek>();
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    if (siMardi.HasValue)
                        lstDay.Add(siMardi.Value);
                    if (siMercredi.HasValue)
                        lstDay.Add(siMercredi.Value);
                    if (siJeudi.HasValue)
                        lstDay.Add(siJeudi.Value);
                    if (siVendredi.HasValue)
                        lstDay.Add(siVendredi.Value);
                    if (siSamedi.HasValue)
                        lstDay.Add(siSamedi.Value);
                    if (siDimanche.HasValue)
                        lstDay.Add(siDimanche.Value);
                    if (siLundi.HasValue)
                        lstDay.Add(siLundi.Value);
                    break;
                case DayOfWeek.Tuesday:
                    if (siMercredi.HasValue)
                        lstDay.Add(siMercredi.Value);
                    if (siJeudi.HasValue)
                        lstDay.Add(siJeudi.Value);
                    if (siVendredi.HasValue)
                        lstDay.Add(siVendredi.Value);
                    if (siSamedi.HasValue)
                        lstDay.Add(siSamedi.Value);
                    if (siDimanche.HasValue)
                        lstDay.Add(siDimanche.Value);
                    if (siLundi.HasValue)
                        lstDay.Add(siLundi.Value);
                    if (siMardi.HasValue)
                        lstDay.Add(siMardi.Value);
                    break;
                case DayOfWeek.Wednesday:
                    if (siJeudi.HasValue)
                        lstDay.Add(siJeudi.Value);
                    if (siVendredi.HasValue)
                        lstDay.Add(siVendredi.Value);
                    if (siSamedi.HasValue)
                        lstDay.Add(siSamedi.Value);
                    if (siDimanche.HasValue)
                        lstDay.Add(siDimanche.Value);
                    if (siLundi.HasValue)
                        lstDay.Add(siLundi.Value);
                    if (siMardi.HasValue)
                        lstDay.Add(siMardi.Value);
                    if (siMercredi.HasValue)
                        lstDay.Add(siMercredi.Value);
                    break;
                case DayOfWeek.Thursday:
                    if (siVendredi.HasValue)
                        lstDay.Add(siVendredi.Value);
                    if (siSamedi.HasValue)
                        lstDay.Add(siSamedi.Value);
                    if (siDimanche.HasValue)
                        lstDay.Add(siDimanche.Value);
                    if (siLundi.HasValue)
                        lstDay.Add(siLundi.Value);
                    if (siMardi.HasValue)
                        lstDay.Add(siMardi.Value);
                    if (siMercredi.HasValue)
                        lstDay.Add(siMercredi.Value);
                    if (siJeudi.HasValue)
                        lstDay.Add(siJeudi.Value);
                    break;
                case DayOfWeek.Friday:
                    if (siSamedi.HasValue)
                        lstDay.Add(siSamedi.Value);
                    if (siDimanche.HasValue)
                        lstDay.Add(siDimanche.Value);
                    if (siLundi.HasValue)
                        lstDay.Add(siLundi.Value);
                    if (siMardi.HasValue)
                        lstDay.Add(siMardi.Value);
                    if (siMercredi.HasValue)
                        lstDay.Add(siMercredi.Value);
                    if (siJeudi.HasValue)
                        lstDay.Add(siJeudi.Value);
                    if (siVendredi.HasValue)
                        lstDay.Add(siVendredi.Value);
                    break;
                case DayOfWeek.Saturday:
                    if (siDimanche.HasValue)
                        lstDay.Add(siDimanche.Value);
                    if (siLundi.HasValue)
                        lstDay.Add(siLundi.Value);
                    if (siMardi.HasValue)
                        lstDay.Add(siMardi.Value);
                    if (siMercredi.HasValue)
                        lstDay.Add(siMercredi.Value);
                    if (siJeudi.HasValue)
                        lstDay.Add(siJeudi.Value);
                    if (siVendredi.HasValue)
                        lstDay.Add(siVendredi.Value);
                    if (siSamedi.HasValue)
                        lstDay.Add(siSamedi.Value);
                    break;
                case DayOfWeek.Sunday:
                    if (siLundi.HasValue)
                        lstDay.Add(siLundi.Value);
                    if (siMardi.HasValue)
                        lstDay.Add(siMardi.Value);
                    if (siMercredi.HasValue)
                        lstDay.Add(siMercredi.Value);
                    if (siJeudi.HasValue)
                        lstDay.Add(siJeudi.Value);
                    if (siVendredi.HasValue)
                        lstDay.Add(siVendredi.Value);
                    if (siSamedi.HasValue)
                        lstDay.Add(siSamedi.Value);
                    if (siDimanche.HasValue)
                        lstDay.Add(siDimanche.Value);
                    break;
            }
            return lstDay;
        }
    }
}
