using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class MessageService
    {
        private DiscordSocketClient _client;
        private readonly JellyfinService _jellyfinService;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public MessageService(DiscordSocketClient client, JellyfinService jellyfinService)
        {
            _client = client;
            _jellyfinService = jellyfinService;

            _client.Ready += Ready;            
            _client.UserLeft += UserLeft;            
            _client.MessageUpdated += MessageUpdated;               
        }


        #region Client
        public async Task MessageUpdated(Cacheable<IMessage, ulong> msgBefore, SocketMessage msgAfter, ISocketMessageChannel channel)
        {
            if (!IsStaffMsg(msgAfter))
            {
                // If the message was not in the cache, downloading it will result in getting a copy of `after`.
                var msg = await msgBefore.GetOrDownloadAsync();

                if(msg.Content != msgAfter.Content)
                    Console.WriteLine($"{msgAfter.Author.Username} edit : \"{msg}\" ---> \"{msgAfter}\" from {channel.Name}");
            }

        }

        /// <summary>
        /// When guild data has finished downloading (+state : Ready)
        /// </summary>
        /// <returns></returns>
        public Task Ready()
        {
            SendLatency();

            // method qui va supp dernier message si +2h jellyfin            

            return Task.CompletedTask;
        }


        /// <summary>
        /// When a User left the Guild
        /// </summary>
        /// <param name="guildUser"></param>
        /// <returns></returns>
        private async Task UserLeft(SocketGuildUser guildUser)
        {
            string user = guildUser.Username + '#' + guildUser.Discriminator;
            string joinedAt = Helper.ConvertToSimpleDate(guildUser.JoinedAt.Value);                                    
            string message = $"{user} left Zderland ! This person joined at {joinedAt}";

            ISocketMessageChannel channel = Helper.GetSocketMessageChannel(guildUser.Guild, "log");

            if (channel != null)
                await channel.SendMessageAsync(message);

            return;
        }        
        #endregion

        #region Reaction
        public async Task AddReactionVu(IUserMessage message)
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
        #endregion

        #region Message
        public async Task SendLatency()
        {
            //_client.Latency
            log.Info($"");
        }

        public  async Task SendJellyfinRefused(ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync("Un lien est déjà disponible un peu plus haut mon brave !");
            return;
        }
        #endregion


        private static bool IsStaffMsg(SocketMessage msg)
        {            
            return (msg.Author.IsBot || msg.Author.Username.StartsWith("Vince"));
        }
    }
}
