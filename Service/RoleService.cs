using Discord;
using Discord.WebSocket;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class RoleService
    {
        private static string _readTheRulesRole = "🥉";
        //private static readonly ulong _rulesMsgId = 847145384387411989;
        //private static readonly ulong _rolesMsgId = 847148020767522886;
        //private static readonly ulong _rulesChannelId = 846694705177165864;
        //private static readonly ulong _rolesChannelId = 846714456788172800;

        private IRole _IRoleRules = null;
        private DiscordSocketClient _client;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public RoleService(DiscordSocketClient client)
        {
            _client = client;
            _client.GuildMembersDownloaded += GuildMembersDownloaded;
        }



        private async Task GuildMembersDownloaded(SocketGuild arg)
        {
            await CheckRole();
        }        
        private async Task CheckRole()
        {
            if (_IRoleRules == null) _IRoleRules = Helper.GetRole(_client, _readTheRulesRole);
            await CheckRules();
            //await CheckAttribution(); ////////////////////////////////////////////////////////////////////////////
        }

        private Task CheckAttribution()
        {
            // MAX 20reac / message
            throw new NotImplementedException();
        }

        private async Task CheckRules()
        {
            var channelRules = Helper.GetSocketMessageChannel(_client, "rules");

            IReadOnlyCollection<IMessage> iMsg = channelRules.GetMessagesAsync(1).FirstAsync().Result;
            IMessage msg = iMsg.First();

            List<SocketGuildUser> allUsers = Helper.GetZderLand(_client).Users.ToList();
            allUsers.RemoveAll(x => x.IsBot);

            foreach (var user in allUsers)
            {
                await user.RemoveRoleAsync(_IRoleRules);//purge
            }

            List<IReadOnlyCollection<IUser>> reactListUsers = msg.GetReactionUsersAsync(msg.Reactions.FirstOrDefault().Key, 1000).ToListAsync().Result;

            foreach (var userLst in reactListUsers)
            {
                var okUserslist = userLst.ToList();

                foreach (var okUser in okUserslist)
                {
                    var subject = allUsers.First(x => x.Username == okUser.Username);
                    await subject.AddRoleAsync(_IRoleRules); //add
                }
            }
        }        
    }
}
