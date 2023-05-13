using Discord;
using Discord.Rest;
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
        private const long _vinceId = 312317884389130241;
        private const long _vinceBisId = 493020872303443969;                
        private DiscordSocketClient _client;
        private static ulong _saloonVoiceId = 493036345686622210;
        private static ulong _squadVoiceId = 1007423970670297178;
        private static ulong _squadTmpVoiceId = ulong.MinValue;        
        private static ulong _vocalCategoryId = 493018545089806337;
        private bool _isSquadOn = false;        

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MessageService(DiscordSocketClient client)
        {
            _client = client;                                               
            _client.UserLeft += UserLeft;                                  
            _client.MessageReceived += MessageReceived;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;                   
        }

        //in = arg2 unknown // out = arg3 unknown
        private async Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)        
        {                        
            //      RequestLive PART            
            if (arg3.VoiceChannel != null && arg1.Id == _vinceBisId)//Compte Deaf IN
            {
                List<SocketGuildUser> targets = arg3.VoiceChannel.ConnectedUsers.ToList();

                SocketGuildUser indexMe = targets.FirstOrDefault(x => x.Id == _vinceBisId); 
                if (indexMe != null)//I'm in
                    targets.Remove(indexMe);

                await AskForLive(targets);                
            }

            //      NewSquad PART                        
            var guild = _client.Guilds.First();

            if (arg3.VoiceChannel != null && !_isSquadOn && arg3.VoiceChannel.Id == _squadVoiceId)
            {
                _isSquadOn = true;

                //New channel                
                RestVoiceChannel newVoice = guild.CreateVoiceChannelAsync("🎮︱Squad bis", props => props.CategoryId = _vocalCategoryId).Result;
                _squadTmpVoiceId = newVoice.Id;
            }
            if(_isSquadOn && arg2.VoiceChannel != null)
            {
                if (arg2.VoiceChannel.Id == _squadTmpVoiceId || arg2.VoiceChannel.Id == _squadVoiceId) //leave
                {
                    //si plus personne dans les 2 --> delete new + isSquad
                    if (guild.VoiceChannels.First(x => x.Id == _squadVoiceId).ConnectedUsers.Count == 0 && 
                        guild.VoiceChannels.First(x => x.Name == "🎮︱Squad bis").ConnectedUsers.Count == 0)
                    {
                        await guild.VoiceChannels.First(x => x.Id == _squadTmpVoiceId).DeleteAsync();
                        _isSquadOn = false;
                    }
                }
            }
        }

        #region Client
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
            
            var modoChannel = Helper.GetSocketMessageChannelModo(_client);

            if (modoChannel != null)
                await modoChannel.SendMessageAsync(message);            
        }

        private Task MessageReceived(SocketMessage arg)
        {
            //DM from User
            if (arg.Source == MessageSource.User && arg.Channel.Name.StartsWith('@'))
            {
                string message = $"<@{arg.Author.Id}> *says* : " + arg.Content ;
                SendToLeader(message);
                AddReactionRobot((SocketUserMessage)arg);
            }
                
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
            var alarm = Emote.Parse(Helper.GetAlarmEmote()) ;            
            await message.AddReactionAsync(alarm);
        }

        public async Task AddDoneReaction(SocketUserMessage message)
        {
            await message.RemoveAllReactionsAsync();

            var check = Emote.Parse(Helper.GetDoneEmote());
            await message.AddReactionAsync(check);
        }
        #endregion

        #region Message
        public async Task AskForLive(List<SocketGuildUser> targets)
        {
            foreach (SocketGuildUser user in targets)
            {
                // check if the user is playing a game and not streaming
                if (!user.IsStreaming && user.Activities.Count > 0)   
                {
                    if(user.Activities.FirstOrDefault().Type == ActivityType.Playing)
                    {
                        log.Info($"AskForLive {user.Activities.First().ToString()} to {user.Username}");

                        // TODO fix ask ppl who just have emoji status
                        await user.SendMessageAsync($"Hello {user.Username}, (sauf erreur de ma part) Vince m'envoie ici alors voici un GIF symbolisant une demande de Stream :\n" +
                            $"https://cdn.discordapp.com/attachments/617462663374438411/1081981535688859678/live.gif");

                        // wait for a short period of time before sending the next message (to avoid rate limiting)
                        await Task.Delay(TimeSpan.FromSeconds(0.5));
                    }                  
                }
            }
        }

        internal void SendToLeader(string message)
        {
            var leader = _client.GetUser(_vinceId);
            leader.SendMessageAsync(message);            
        }

        #region Control Message
        internal async Task CommandNotAuthorizeHere(ISocketMessageChannel channel, MessageReference reference, ulong idChannelWhereLegit)
        {
            await channel.SendMessageAsync($"L'utilisation de cette commande est limitée au channel <#{idChannelWhereLegit}>", messageReference: reference);
        }

        internal async Task CommandForbidden(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"L'utilisation de cette commande est interdite !", messageReference: reference);
        }

        internal async Task SendJellyfinNotAuthorizeHere(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"⚠️ Pour des raisons de sécurité l'utilisation de Jellyfin" +
                $" est limitée au channel <#816283362478129182>", messageReference: reference);            
        }

        internal async Task SendJellyfinAlreadyInUse(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"Attention Jellyfin est déjà en cours d'utilisation ! Merci de regarder les PINS", messageReference: reference);
        }

        internal async Task JellyfinNotAvailable(ISocketMessageChannel channel, MessageReference reference)
        {
            await channel.SendMessageAsync($"La base de donnée est indisponible pour le moment.\n " +
                $"Pour rappel, /ping mets à jour mon statut", messageReference: reference);
        }
        #endregion        
        #endregion        
    }
}