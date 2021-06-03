using Discord;
using Discord.WebSocket;
using log4net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BoTools.Service
{
    public class RoleService
    {
        private static ulong _readTheRulesId = 847048535799234560;
        private static ulong _vipId = 322490732885835776;
        private static ulong _valideId = 344912149728067584;

        private bool _connexion = true;
        private IRole _IRoleRules = null;
        private List<IRole> _IRolesAttribution = new List<IRole>();
        List<SocketGuildUser> _allUsers = new List<SocketGuildUser>();
        private Dictionary<IRole, string> _roleToEmoteGames = new Dictionary<IRole, string>();
        private Dictionary<IRole, string> _roleToEmoteSpecial = new Dictionary<IRole, string>();

        private DiscordSocketClient _client;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public RoleService(DiscordSocketClient client)
        {
            _client = client;
            _client.GuildMembersDownloaded += GuildMembersDownloaded;
        }


        private async Task GuildMembersDownloaded(SocketGuild arg)
        {
            log.Info($"| GuildMembersDownloaded first={_connexion}");
            if (_connexion) 
            {
                await CheckRoles();
                _connexion = false;
            }
            log.Info("| GuildMembersDownloaded out");
        }        

        public async Task CheckRoles()
        {
            if (_IRoleRules == null) _IRoleRules = Helper.GetRoleById(_client, _readTheRulesId);

            if (_allUsers.Count == 0)
            {
                _allUsers = Helper.GetZderLand(_client).Users.ToList();
                _allUsers.RemoveAll(x => x.IsBot);
            }

            await CheckRules();

            if (_IRolesAttribution.Count == 0) 
            {
                _IRolesAttribution = Helper.GetRolesAttribution(_client).ToList();
                _roleToEmoteGames = Helper.RoleToEmoteGames(_IRolesAttribution);
                _roleToEmoteSpecial = Helper.RoleToEmoteSpecial(_IRolesAttribution);
            }
            CheckAttribution();
        }

        private Task CheckAttribution()
        {
            var chrono = new Stopwatch();
            chrono.Start();            

            var channelRules = Helper.GetSocketMessageChannel(_client, "rôles");
            var iMsgs = channelRules.GetMessagesAsync(2).ToListAsync().Result;

            IMessage msgGames = null;
            IMessage msgSpecial = null;

            foreach (var msg in iMsgs.First())
            {
                if (msg.Content.Contains("Plateforme")) msgGames = msg;
                if (msg.Content.Contains("Spécial")) msgSpecial = msg;
            }

            foreach (var reaction in msgSpecial.Reactions)
            {
                List<IReadOnlyCollection<IUser>> reactListUsers = msgSpecial.GetReactionUsersAsync(reaction.Key, 1000).ToListAsync().Result;
                var roleToAssign = _roleToEmoteSpecial.First(x => x.Value == reaction.Key.Name).Key;
                log.Info($"s_roleToAssign : {roleToAssign}");

                foreach (var userLst in reactListUsers)
                {
                    var okUserslist = userLst.ToList();

                    foreach (var okUser in okUserslist)
                    {
                        if (okUser.Id != 493020872303443969)
                        {
                            SocketGuildUser subject = _allUsers.First(x => x.Id == okUser.Id);                                                       
                            subject.AddRoleAsync(roleToAssign);
                            log.Info($"SPE_{roleToAssign.Name} add for {subject.Username}");
                        }
                    }
                }
            }

            foreach (var reaction in msgGames.Reactions)
            {
                List<IReadOnlyCollection<IUser>> reactListUsers = msgGames.GetReactionUsersAsync(reaction.Key, 1000).ToListAsync().Result;
                var roleToAssign = _roleToEmoteGames.First(x => x.Value == reaction.Key.Name).Key;
                log.Info($"g_roleToAssign : {roleToAssign}");

                foreach (var userLst in reactListUsers)
                {
                    var okUserslist = userLst.ToList();

                    foreach (var okUser in okUserslist)
                    {
                        if (okUser.Id != 493020872303443969)
                        {
                            SocketGuildUser subject = _allUsers.First(x => x.Id == okUser.Id);
                            subject.AddRoleAsync(roleToAssign);
                            log.Info($"GAME_{roleToAssign.Name} add for {subject.Username}");
                        }
                    }
                }
            }

            chrono.Stop();
            log.Info($"CheckAttribution done in {chrono.ElapsedMilliseconds}ms");
            return Task.CompletedTask;
        }

        private async Task CheckRules()
        {
            var chrono = new Stopwatch();
            chrono.Start();
            
            var channelRules = Helper.GetSocketMessageChannel(_client, "rules");
            IReadOnlyCollection<IMessage> iMsg = channelRules.GetMessagesAsync(1).FirstAsync().Result;
            IMessage msg = iMsg.First();

            List<IReadOnlyCollection<IUser>> reactListUsers = msg.GetReactionUsersAsync(msg.Reactions.FirstOrDefault().Key, 1000).ToListAsync().Result;

            foreach (var userLst in reactListUsers)
            {
                var okUserslist = userLst.ToList();

                foreach (var okUser in okUserslist)
                {
                    if (okUser.Id != 493020872303443969)
                    {
                        var subject = _allUsers.First(x => x.Id == okUser.Id);
                        subject.AddRoleAsync(_IRoleRules);
                        log.Info($"CheckRules done for {subject.Username}");
                    }
                }
            }                     

            chrono.Stop();
            log.Info($"CheckRules done in {chrono.ElapsedMilliseconds}ms");
        }

        public async Task UpdateListUser()
        {
            await _client.DownloadUsersAsync(Helper.GetZderLands(_client)); // DL all user

            _allUsers = Helper.GetZderLand(_client).Users.ToList();
            _allUsers.RemoveAll(x => x.IsBot);
        }

        public Task PurgeRoles()
        {
            foreach (var user in _allUsers)
            {
                if (!user.Username.Trim().Contains("Vince"))
                {
                    user.RemoveRoleAsync(_IRoleRules);
                    user.RemoveRolesAsync(_IRolesAttribution);
                }
            }
            return Task.CompletedTask;
        }

        #region Update Live
        internal void RulesReactionRemoved(ulong userId)
        {
            var subject = _allUsers.First(x => x.Id == userId);
            subject.RemoveRoleAsync(_IRoleRules);
        }

        internal void SpecialReactionRemoved(SocketReaction reaction)
        {
            var subject = _allUsers.First(x => x.Id == reaction.User.Value.Id);
            var roleToAssign = _roleToEmoteSpecial.First(x => x.Value == reaction.Emote.Name).Key;
            subject.RemoveRoleAsync(roleToAssign);
        }

        internal void GamesReactionRemoved(SocketReaction reaction)
        {
            var subject = _allUsers.First(x => x.Id == reaction.User.Value.Id);
            var roleToAssign = _roleToEmoteGames.First(x => x.Value == reaction.Emote.Name).Key;
            subject.RemoveRoleAsync(roleToAssign);
        }

        internal void RulesReactionAdded(ulong userId)
        {
            var subject = _allUsers.First(x => x.Id == userId);
            subject.AddRoleAsync(_IRoleRules);
        }

        internal void SpecialReactionAdded(SocketReaction reaction)
        {
            var subject = _allUsers.First(x => x.Id == reaction.User.Value.Id);
            var roleToAssign = _roleToEmoteSpecial.First(x => x.Value == reaction.Emote.Name).Key;
            subject.AddRoleAsync(roleToAssign);
        }

        internal void GamesReactionAdded(SocketReaction reaction)
        {
            var subject = _allUsers.First(x => x.Id == reaction.User.Value.Id);
            var roleToAssign = _roleToEmoteGames.First(x => x.Value == reaction.Emote.Name).Key;
            subject.AddRoleAsync(roleToAssign);
        }
        #endregion        
    }
}
