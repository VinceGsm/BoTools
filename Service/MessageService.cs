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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public async Task MessageReceived(SocketMessage msg)
        {
            //throw new NotImplementedException();
            return;
        }

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

        private static bool IsStaffMsg(SocketMessage msg)
        {
            return (msg.Author.IsBot || msg.Author.Username.StartsWith("Vince"));
        }
    }
}
