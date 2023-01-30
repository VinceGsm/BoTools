﻿using BoTools.Service;
using Discord;
using Discord.Interactions;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Module
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private const ulong _idOpRole = 552134779210825739;
        private const ulong _idModoRole = 322489502562123778;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EventService _eventService;


        public InteractionModule(EventService eventService)
        {
            _eventService = eventService;
        }
        

        
        [SlashCommand("ping",        // Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            "BoTools es-tu là ?",    // Descriptions can have a max length of 100.
            false, RunMode.Async)]     
        public async Task HandlePingPongInteraction()
        {
            log.Info("HandlePingPongInteraction IN");
            await RespondAsync("PONG !");
            log.Info("HandlePingPongInteraction OUT");
        }


        [RequireRole(roleId: _idOpRole)]
        [SlashCommand("feedback_one-piece", "Comment était le dernier épisode de One Piece ?")]
        public async Task HandleRateOpCommand(
            [Choice("1. Mauvais !", $"💩💩💩"),
            Choice("2. Ennuyant", $"💤💤💤"),
            Choice("3. Passable", $"👁👁👁"),
            Choice("4. Sympa", $"👍👍👍"),
            Choice("5. Epoustouflant", $"🔥🔥🔥"),
            Choice("6. Légendaire !", $"❤️❤️❤️")] string feedback)
        {        
            log.Info("HandleRateOpCommand IN");

            string nameFeedback = string.Empty;
            switch (feedback)
            {
                case $"💩💩💩":
                    nameFeedback = "1. Mauvais !";
                    break;
                case $"💤💤💤":
                    nameFeedback = "2. Ennuyant";
                    break;
                case $"👁👁👁":
                    nameFeedback = "3. Passable";
                    break;
                case $"👍👍👍":
                    nameFeedback = "4. Sympa";
                    break;
                case $"🔥🔥🔥":
                    nameFeedback = "5. Epoustouflant";
                    break;
                case $"❤️❤️❤️":
                    nameFeedback = "6. Légendaire !";
                    break;
            }

            string urlIcon = "";
            switch (Int32.Parse(nameFeedback.First().ToString()))
            {
                case 1:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290456782557184/vomit.png";
                    break;
                case 2:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290442140237834/bad-review.png";
                    break;
                case 3:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290427002982491/mood.png";
                    break;
                case 4:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290410867495003/good-review.png";
                    break;
                case 5:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290390063759420/satisfaction.png";
                    break;
                case 6:
                    urlIcon = "https://cdn.discordapp.com/attachments/617462663374438411/1011290172836552844/heart.png";
                    break;
            }

            var embedBuilder = new EmbedBuilder()
                
                .WithTitle($"Feedback : {nameFeedback.Remove(0, 3)}")
                .WithDescription($"{feedback}")
                .WithColor(Color.DarkBlue)
                .WithImageUrl(urlIcon);

            await RespondAsync(embed: embedBuilder.Build());
            log.Info("HandleRateOpCommand OUT");
        }

        [RequireRole(roleId: _idOpRole)]
        [SlashCommand("feedback_one-piece-lite", "Comment était le dernier épisode de One Piece ?")]
        public async Task HandleRateOpCommandLite(
            [Choice("1. Nullissime", "⭐"),
            Choice("2. Pas ouf", "⭐⭐"),
            Choice("3. Ok", "⭐⭐⭐"),
            Choice("4. Bien", "⭐⭐⭐⭐"),
            Choice("5. Excellent", "⭐⭐⭐⭐⭐")            
            ] string feedback) 
        {
            log.Info("HandleRateOpLiteCommand IN");

            string nameFeedBack = string.Empty;

            switch (feedback)
            {
                case $"⭐":
                    nameFeedBack = "NULLISSIME";
                    break;
                case $"⭐⭐":
                    nameFeedBack = "PAS OUF";
                    break;
                case $"⭐⭐⭐":
                    nameFeedBack = "OK";
                    break;
                case $"⭐⭐⭐⭐":
                    nameFeedBack = "BIEN";
                    break;
                case $"⭐⭐⭐⭐⭐":
                    nameFeedBack = "EXCELLENT";
                    break;
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Feedback : {nameFeedBack}")
                .WithDescription($"{feedback}")
                .WithColor(Color.DarkRed);

            await RespondAsync(embed: embedBuilder.Build());
            log.Info("HandleRateOpLiteCommand OUT");
        }


        [SlashCommand("roles", "Affiche la liste des rôles principaux du server", false, RunMode.Async)]        
        public async Task HandleMainRolesCommand()
        {
            log.Info("HandleMainRolesCommand IN");

            List<string> roles = new List<string>();
            roles.Add("<@&689144324939710527>");
            roles.Add("<@&322489502562123778>");
            roles.Add("<@&322490732885835776>");
            roles.Add("<@&344912149728067584>");
            roles.Add("<@&847048535799234560>");

            // We remove the everyone role and select the mention of each role.
            var roleList = string.Join("\n", roles);            

            var embedBuiler = new EmbedBuilder()
                //.WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithTitle("Rôles principaux de Zderland :")
                .WithDescription(roleList)
                .WithColor(Color.Green)
                .WithImageUrl(Helper.GetZderLandIconUrl());

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
            log.Info("HandleMainRolesCommand OUT");
        }

        [RequireRole(roleId: _idModoRole)]
        [SlashCommand("event-serie-hebdo", "Créé tout les events pour des episodes hebdo", false, RunMode.Async)] 
        public async Task HandleEventSeriesHebdoCommand(string name, int numFirstEpisode, int numLastEpisode, DayOfWeek dayOfWeek, Double hour)
        {
            log.Info("HandleEventSeriesHebdoCommand IN");

            string msg = "N'oubliez pas de cliqué sur la cloche de l'event afin d'être notifié lorsqu'il commence !";               

            int nbEp = numLastEpisode - numFirstEpisode +1;

            var embedBuiler = new EmbedBuilder()                
                .WithTitle($"Création de {nbEp} events {name} en cours...")
                .WithDescription(msg)
                .WithColor(Color.Green)
                .WithImageUrl(Helper.GetZderLandIconUrl());            

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: false);

            _eventService.CreateEventHebdoSerie(name, numFirstEpisode, nbEp, dayOfWeek, hour);

            log.Info("HandleEventSeriesHebdoCommand OUT");
        }

        [RequireRole(roleId: _idModoRole)]
        [SlashCommand("event-enserie", "Créé X event : selectionner les jours autre que la cible (s'il y en a)", false, RunMode.Async)]
        public async Task HandleEventSeriesCommand(string name, int nbSession, Double hour, bool isIrlEvent,
        DayOfWeek? siLundi=null, DayOfWeek? siMardi=null, DayOfWeek? siMercredi=null, DayOfWeek? siJeudi=null, DayOfWeek? siVendredi=null, DayOfWeek? siSamedi=null, DayOfWeek? siDimanche=null)        
        {
            log.Info("HandleEventSeriesCommand IN");

            string msg = "N'oubliez pas de cliqué sur la cloche de l'event afin d'être notifié lorsqu'il commence !";            
            
            var embedBuiler = new EmbedBuilder()
                .WithTitle($"Création de {nbSession} events {name} en cours...")
                .WithDescription(msg)
                .WithColor(Color.Green)
                .WithImageUrl(Helper.GetZderLandIconUrl());

            await RespondAsync(embed: embedBuiler.Build(), ephemeral: false);            

            _eventService.CreateEventEnSerie(name, hour, nbSession, isIrlEvent,
                siLundi,siMardi,siMercredi,siJeudi,siVendredi,siSamedi,siDimanche);

            log.Info("HandleEventSeriesCommand OUT");
        }
    }
}
