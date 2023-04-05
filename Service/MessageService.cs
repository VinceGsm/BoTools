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
        Dictionary<string, DateTime> _birthDays = null;
        DateTime? _onGoingBirthday = null;
        private DiscordSocketClient _client;
        private static ulong _saloonVoiceId = 493036345686622210;
        private static ulong _birthdayId = 1052530092082995201;
        private IRole _IRoleBirthday = null;

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MessageService(DiscordSocketClient client)
        {
            _client = client;                                               
            _client.UserLeft += UserLeft;                                  
            _client.MessageReceived += MessageReceived;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;       
            

            if (_birthDays == null)
                _birthDays = Helper.GetBirthDays();
        }

        //in = arg2 unknown // out = arg3 unknown
        private async Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)        
        {

            //      BIRTHDAY PART
            if (_IRoleBirthday == null) _IRoleBirthday = Helper.GetRoleById(_client, _birthdayId);

            if (_onGoingBirthday == null) //pas anniv en cours                         
                await CheckBirthday();            
            else
            {
                if (_onGoingBirthday != DateTime.Today) //anniv en cours != ajd ?
                    await CheckBirthday();
            }
            
            //      RequestLive PART            
            if (arg3.VoiceChannel != null && arg1.Id == _vinceBisId)//Compte Deaf IN
            {
                List<SocketGuildUser> targets = arg3.VoiceChannel.ConnectedUsers.ToList();

                SocketGuildUser indexMe = targets.FirstOrDefault(x => x.Id == _vinceBisId); 
                if (indexMe != null)//I'm in
                    targets.Remove(indexMe);

                await AskForLive(targets);                
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

        public async Task AddReactionBirthDay(IMessage message)
        {            
            var bravo = Emote.Parse(Helper.GetBravoEmote());
            // --> 🎂
            Emoji cake = new Emoji("\uD83C\uDF82");

            await message.AddReactionAsync(cake);
            await message.AddReactionAsync(bravo);
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

        public async Task CleanLastMsgChannel(ulong idTargetChannel)
        {
            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, idTargetChannel);
            IReadOnlyCollection<IMessage> lasthundredMsg = channel.GetMessagesAsync(100).FirstAsync().Result;            

            foreach (var msg in lasthundredMsg)
            {
                await channel.DeleteMessageAsync(msg);
            }            
        }

        public async Task CheckBirthday()
        {
            string msgStart = $"@here {Helper.GetPikachuEmote()} \n" +
                        $"Vince me souffle dans l'oreille que c'est l'anniversaire de";

            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(_client, Helper._idGeneralChannel);
            
            if (_birthDays == null)
                log.Error("list birthdays null !");
            else
            {
                bool isSomeoneBD = _birthDays.ContainsValue(DateTime.Today);

                if (isSomeoneBD)
                {
                    string idTagTarget = _birthDays.First(x => x.Value == DateTime.Today).Key;                    

                    string message = msgStart + $" <@{idTagTarget}> aujourd'hui !\n" +
                        $"{Helper.GetCoeurEmote()} sur toi";

                    if (channel != null)
                    {
                        _onGoingBirthday = DateTime.Today;
                        var userTarget = Helper.GetZderLand(_client).Users.First(x => x.Id == Convert.ToUInt64(idTagTarget.Remove(0,1)));
                        userTarget.AddRoleAsync(_IRoleBirthday);

                        var res = (IMessage)channel.SendMessageAsync(message).Result;
                        await AddReactionBirthDay(res);
                    }
                    else log.Error("Can't wish HB because general was not found");
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
            await channel.SendMessageAsync($"La base de donnée est indisponible pour le moment. C'est pourtant écrit dans mon statut...\n " +
                $"Pour rappel, /ping mets à jour mon statut", messageReference: reference);
        }

        //internal async Task SendNgrokReset(ISocketMessageChannel channel)
        //{
        //    await channel.SendMessageAsync($"{Helper.GetAlarmEmote()} Un nouveau lien va être généré ! {Helper.GetAlarmEmote()}\n" +
        //        $"|| https://discord.com/channels/312966999414145034/816283362478129182/1010199767785160865 ||\n" +            
        //        $"*En cas de soucis direct avec Jellyfin merci de contacter Vince*");
        //}
        #endregion
        #endregion        

        #region Embed
        /// <summary>
        /// Message Embed with link
        /// </summary>
        /// <param name="userMsg"></param>
        /// <param name="ngRockUrl"></param>
        /// <returns></returns>
        public EmbedBuilder MakeJellyfinMessageBuilder(SocketUserMessage userMsg, string ngRockUrl)
        {
            log.Info($"IMG_url: " + Helper._JellyfinImgUrl);
            return new EmbedBuilder
            {
                Url = ngRockUrl,
                Color = Color.DarkRed,
                ImageUrl = Helper._JellyfinImgUrl,
                ThumbnailUrl = Helper._boToolsGif,

                Title = $"{Helper.GetVerifiedEmote()}︱Cliquez ici︱{Helper.GetVerifiedEmote()}",                
                Description = $"{Helper.GetCoinEmote()}  En stream avec **Jellyfin Media Player** sur PC\n" +
                    $"{Helper.GetCoinEmote()}  En **DL** avec Google CHrome sur PC\n" +
                    $"{Helper.GetCoinEmote()}  ERR_NGROK = relancer **$Jellyfin** \n"+
                    $"{Helper.GetCoinEmote()}  / à venir",

                Author = new EmbedAuthorBuilder { Name = "Jellyfin requested by " + userMsg.Author.Username, IconUrl = userMsg.Author.GetAvatarUrl() },
                Footer = GetFooterBuilder()
            };
        }

        private EmbedFooterBuilder GetFooterBuilder()
        {
            return new EmbedFooterBuilder
            {
                IconUrl = Helper._urlAvatarVince,
                Text = $"Powered with {Helper.GetCoeurEmoji()} by Vince"
            };
        }

        public async Task UnPinLastJelly(List<RestMessage> pinneds)
        {
            try
            {
                var lastPin = pinneds.First() as IUserMessage;
                if (lastPin != null)
                {
                    if (lastPin.Content.StartsWith('$'))
                        await lastPin.UnpinAsync();
                    else
                    {
                        var nextPin = pinneds.Skip(1).OfType<IUserMessage>().FirstOrDefault(x => x.Content.StartsWith('$'));
                        await nextPin.UnpinAsync();
                    }
                }
            }
            catch (Exception ex) { log.Warn("UnPinLastJelly"); log.Error(ex); }                            
        }

        #endregion
    }
}