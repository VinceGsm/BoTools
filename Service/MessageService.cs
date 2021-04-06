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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public MessageService(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageDeleted += MessageDeleted;
            _client.MessageUpdated += MessageUpdated;
            //_client.MessageReceived += ;
        }


        #region Client
        public Task MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public async Task MessageUpdated(Cacheable<IMessage, ulong> msgBefore, SocketMessage msgAfter, ISocketMessageChannel channel)
        {
            if (!IsStaffMsg(msgAfter))
            {
                // If the message was not in the cache, downloading it will result in getting a copy of `after`.
                var msg = await msgBefore.GetOrDownloadAsync();

                Console.WriteLine($"{msgAfter.Author.Username} edit : \"{msg}\" ---> \"{msgAfter}\" from {channel.Name}");
            }

        }
        #endregion


        public async Task AddReactionVu(IUserMessage message)
        {
            // --> 👀
            Emoji vu = new Emoji("\uD83D\uDC40");
            await message.AddReactionAsync(vu);
            Console.WriteLine($"VU done");
        }
        
        //


        private static bool IsStaffMsg(SocketMessage msg)
        {
            return false; //DEBUG
            return (msg.Author.IsBot || msg.Author.Username.StartsWith("Vince"));
        }
    }
}
