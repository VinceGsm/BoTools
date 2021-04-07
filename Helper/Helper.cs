using Discord.WebSocket;
using log4net;
using System;
using System.Linq;
using System.Reflection;

namespace BoTools
{
    public static class Helper
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static ISocketMessageChannel GetSocketMessageChannel(SocketGuild guild, string channelName)
        {
            ISocketMessageChannel xx = (ISocketMessageChannel)
                guild.Channels.ToList().Where(x => x.Name.Contains(channelName)).First();

            if (xx == null)
                log.Error($"GetSocketMessageChannel : no channel {channelName} in {guild.Name}");
            return xx;

        }

        public static string ConvertToSimpleDate(DateTimeOffset dateTimeOffset)
        {
            DateTime joinedDate = DateTime.Parse(dateTimeOffset.ToString());
            return joinedDate.Date.ToString().Replace("00:00:00", joinedDate.TimeOfDay.ToString());
        }
    }
}
