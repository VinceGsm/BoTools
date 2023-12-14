﻿using Discord;
using Discord.Rest;
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
        public DiscordSocketClient _client;        
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EventService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task CreateNextOnePiece(bool notif)
        {
            int nextNumOnePiece = GetNextNumOnePiece();
            var nameEvent = $"One Piece {nextNumOnePiece}";  
            log.Debug($"CreateNextOnePiece : {nameEvent}" );

            SocketGuild _serv = Helper.GetZderLand(_client);            
            var eventsAsync = await _serv.GetEventsAsync();
            List<RestGuildEvent> events = eventsAsync.ToList();
            var opChannel = Helper.GetSocketMessageChannel(_client, Helper._idOnePieceChannel) as ITextChannel;
            var activeThreadlst = Helper.GetAllActiveThread(opChannel);            

            if (!events.Any(x => x.Name == nameEvent))
            {
                log.Debug("no next OnePiece already planned");                
                await CreateEventOnePiece(nameEvent, _serv, notif);
                await Helper.ClosedAllActiveThread(opChannel);
                CreateThreadOnePiece(nextNumOnePiece, opChannel);
            }
        }

        private async Task CreateEventOnePiece(string nameEvent, SocketGuild _serv, bool notif)
        {
            try // AWS
            {                
                DateTime today = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);                
                DateTime target = Helper.GetNextWeekday(today, DayOfWeek.Sunday);
                DateTimeOffset startTime = new DateTimeOffset(target.AddHours(21), TimeSpan.FromHours(1)); // 21h (+1 Hiver / +2 Ete)                
                log.Debug(startTime);

                GuildScheduledEventType type = GuildScheduledEventType.Voice;
                string description = "**Venez suivre** l'aventure de **Monkey D. Luffy**, futur Roi des pirates !\n" +
                    "\"Motomeru naraba dokomademo\"";
                ulong? channelId = Helper._idSaloonVoice;
                Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "Onepiece.png"));

                var creation = await _serv.CreateEventAsync(nameEvent, startTime: startTime, type: type, description: description, channelId: channelId, coverImage: coverImage);

                if (notif) { await DirectMessageOnePiece(creation.Id); }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private async Task DirectMessageOnePiece(ulong idEvent)
        {
            string msgOnePiece = $"{Helper._pikachuEmote}\n" +
                $"Un nouvel event One Piece vient d'être créé, clique sur la cloche pour être notifié lorsqu'il commencera {Helper._luffyEmote}\n" +
                $"|| Message envoyé automatiquement une fois par semaine (En cas de problème contacter <@312317884389130241>) ||" +
                $"https://discord.com/events/312966999414145034/{idEvent}";
            List<SocketUser> users = new List<SocketUser>();
            //users.Add(_client.GetUser(Helper._vinceId));
            users.Add(_client.GetUser(Helper._vinceBisId));
            users.Add(_client.GetUser(Helper._antoId));
            users.Add(_client.GetUser(Helper._orelId));
            users.Add(_client.GetUser(Helper._floId));
            foreach (SocketUser user in users)
            {
                await user.SendMessageAsync(msgOnePiece);
            }
        }

        private Task CreateThreadOnePiece(int nextNumOnePiece, ITextChannel opChannel)
        {                     
            opChannel.CreateThreadAsync(nextNumOnePiece.ToString(), autoArchiveDuration:ThreadArchiveDuration.OneWeek);
            log.Info($"Thread OP {nextNumOnePiece} created");
            return Task.CompletedTask;
        }

        public int GetNextNumOnePiece()
        {            
            string html;                     
            var htmlDoc = new HtmlDocument();

            try
            {
                string url = "https://trakt.tv/shows/one-piece";
                log.Info($"{url}");

                using (HttpClient httpClient = new HttpClient())
                {                    
                    html = httpClient.GetStringAsync(url).Result;
                }
                htmlDoc.LoadHtml(html);

                var bricolage = htmlDoc.DocumentNode.SelectNodes("//div[@class='titles']").First().InnerText.Trim().Substring(38,3);

                int.TryParse(bricolage, out int res);                

                return (res == 0) ? 10000 : res +1001;                //error = 10000
            }
            catch(Exception ex) 
            { log.Error(ex.Message); return 0; }
        }

        public async void CreateEventHebdoSerie(string name, int numFirstEpisode, int nbEp, DayOfWeek dayOfWeek, Double hour)
        {
            log.Info("CreateEventHebdoSerie IN");

            SocketGuild _serv = Helper.GetZderLand(_client);

            DateTime now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);            
            DateTime today = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);
            DateTime firstTargetDay = Helper.GetNextWeekday(today, dayOfWeek);
            
            for (int i=0; i<nbEp; i++)
            {
                var nameEvent = $"{name} #{numFirstEpisode}";
                log.Info($"CreateEventHebdoSerie : {nameEvent}");

                try
                {
                    if (i == 0)
                        now = firstTargetDay;
                    else
                        now = Helper.GetNextWeekday(now, dayOfWeek);

                    DateTimeOffset startTime = new DateTimeOffset(now.AddHours(hour), TimeSpan.FromHours(2));
                    GuildScheduledEventType type = GuildScheduledEventType.Voice;
                    string description = "Event créer grâce à la commande **/event-serie-hebdo**";
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

            DateTime today = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);
            int cptWeek = 0;
            
            for (int i = 0; i < nbSession; i++)
            {                
                var nameEvent = $"{name} || #{i + 1}";
                log.Info($"CreateEventEnSerie : {nameEvent}");

                Image? coverImage = new Image(Path.Combine(Environment.CurrentDirectory, @"PNG\", "eventEnSerie.png"));
                try
                {                    
                    today = Helper.GetNextWeekday(today, lstDay[cptWeek]);
                    DateTimeOffset startTime = new DateTimeOffset(today.AddHours(hour), TimeSpan.FromHours(2));

                    if (isIrlEvent)
                    {
                        GuildScheduledEventType type = GuildScheduledEventType.External;
                        var endTime = startTime.AddHours(2);
                        string description = "Event qui fait prendre l'air !";                        

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
