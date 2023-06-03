using Discord;
using Discord.Rest;
using Discord.WebSocket;
using log4net;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class MessageService
    {
        private const long _vinceId = 312317884389130241;
        private const long _vinceBisId = 493020872303443969;                        
        private static ulong _squadVoiceId = 1007423970670297178;                
        private static ulong _vocalCategoryId = 493018545089806337;
        private static ulong _tmpSquadVoiceId = ulong.MinValue;
        private static ulong _tmpReuVoiceId = ulong.MinValue;        

        private DiscordSocketClient _client;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MessageService(DiscordSocketClient client)
        {
            _client = client;                                               
            _client.UserLeft += UserLeft;                                  
            _client.MessageReceived += MessageReceived;
            _client.UserVoiceStateUpdated += UserVoiceStateUpdated;       
        }

        public EmbedBuilder CreateVocalReu(string theme, int nbParticipant)
        {
            EmbedBuilder embed = new EmbedBuilder();
            if (_tmpReuVoiceId != ulong.MinValue)
            {
                embed = new EmbedBuilder()
                   .WithTitle("Une réunion est déjà en cours !")
                   .WithDescription("Relancer la commande une fois qu'elle sera terminée.\n" +
                   "En cas de problème contacter <@312317884389130241>")
                   .WithColor(Color.Red);
            }
            else
            {
                var newVoice = _client.Guilds.First()
                    .CreateVoiceChannelAsync($"🔒︱Réunion {theme}︱⏱", props => {
                        props.Bitrate = 128000;
                        props.UserLimit = nbParticipant;
                        props.CategoryId = _vocalCategoryId;
                    }).Result;
                _tmpReuVoiceId = newVoice.Id;

                embed = new EmbedBuilder()
                    .WithTitle($"{newVoice.Name} est ouvert pour {nbParticipant} participants")
                    .WithDescription($"Il sera supprimé automatiquement par mes soins quand tout le monde aura quitter le vocal.\n" +
                    $"Bon call !")
                    .WithColor(Color.Green);                                
            }
            return embed;
        }

        #region Client        
        private async Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
        {
            //in = arg2 unknown // out = arg3 unknown
            string nameNewChannel = "🎮︱Squad bis";
            var guild = _client.Guilds.First();
            
            if (arg3.VoiceChannel != null) //IN
            {
                //New channel needed               
                if (arg3.VoiceChannel != null && _tmpSquadVoiceId == ulong.MinValue && arg3.VoiceChannel.Id == _squadVoiceId)
                {
                    RestVoiceChannel newVoice = guild.CreateVoiceChannelAsync(nameNewChannel, props => {
                        props.CategoryId = _vocalCategoryId;
                        props.UserLimit = 6;
                        props.Bitrate = 12800;
                    }).Result;                    
                    _tmpSquadVoiceId = newVoice.Id;
                }
            }
            
            if(arg2.VoiceChannel != null) //OUT
            {
                if (_tmpSquadVoiceId != ulong.MinValue)
                {
                    if (arg2.VoiceChannel.Id == _tmpSquadVoiceId || arg2.VoiceChannel.Id == _squadVoiceId) //leave squad
                    {
                        //si plus personne dans les 2 --> delete new
                        if (guild.VoiceChannels.First(x => x.Id == _squadVoiceId).ConnectedUsers.Count == 0 &&
                            guild.VoiceChannels.First(x => x.Name == nameNewChannel).ConnectedUsers.Count == 0)
                        {
                            await guild.VoiceChannels.First(x => x.Id == _tmpSquadVoiceId).DeleteAsync();
                            _tmpSquadVoiceId = ulong.MinValue;
                        }
                    }
                }

                //REU
                if(_tmpReuVoiceId != ulong.MinValue && arg2.VoiceChannel.Id == _tmpReuVoiceId) //leave reu
                {
                    if (guild.VoiceChannels.First(x => x.Id == _tmpReuVoiceId).ConnectedUsers.Count == 0)
                    {
                        await guild.VoiceChannels.First(x => x.Id == _tmpReuVoiceId).DeleteAsync();
                        _tmpReuVoiceId = ulong.MinValue;
                    }
                }                
            }                       
        }

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

        #region Private Message
        internal void SendToLeader(string message)
        {
            var leader = _client.GetUser(_vinceId);
            leader.SendMessageAsync(message);            
        }
        #endregion
    }
}