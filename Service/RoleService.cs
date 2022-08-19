using Discord;
using Discord.WebSocket;
using log4net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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


        #region Update At Start
        private async Task GuildMembersDownloaded(SocketGuild arg)
        {
            log.Info($"| GuildMembersDownloaded first={_connexion}");
            if (_connexion)
            {
                await Helper.SendLatencyAsync(_client);
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
                RolesToEmoteReaction(_IRolesAttribution);

            CheckAttribution();
        }

        private void RolesToEmoteReaction(List<IRole> rolesAttribution)
        {
            _IRolesAttribution = Helper.GetRolesAttribution(_client).ToList();
            FillRolesDicos(_IRolesAttribution);            
        }

        internal void FillRolesDicos(List<IRole> rolesAttribution)
        {            
            foreach (var role in rolesAttribution)
            {
                var potentialName = Regex.Replace(role.Name, @"[^\u0000-\u007F]+", "");                                

                if (string.IsNullOrEmpty(potentialName)) // const + sans char
                {
                    switch (role.Name)
                    {
                        case "👽":
                            _roleToEmoteSpecial.Add(role, "👽");
                            break;
                        case "📹":
                            _roleToEmoteSpecial.Add(role, "📹");
                            break;
                        case "🎵":                            
                            _roleToEmoteSpecial.Add(role, "🎵");
                            break;
                    }
                }
                else
                {
                    switch (role.Name)
                    {
                        // CONTAINS
                        case string name when name.Contains("Anime"):
                        //case "👺 Anime 💘":                        
                            _roleToEmoteSpecial.Add(role, "👺");
                            break;
                        //case "👒 One Piece 💘":
                        case string name when name.Contains("One Piece"):
                            _roleToEmoteSpecial.Add(role, "👒");
                            break;

                        // ENDS                       
                        case string name when name.EndsWith("Creator"):
                            //case "🎥 Content Creator":
                            _roleToEmoteSpecial.Add(role, "🌐");
                            break;
                        case string name when name.EndsWith("Casino"):
                            //case "🎰 Casino":
                            _roleToEmoteSpecial.Add(role, "🎰");
                            break;
                        //case "👾 Games":
                        case string name when name.EndsWith("Games"):
                            _roleToEmoteSpecial.Add(role, "👾");
                            break;
                        //case "💾 Fortnite":
                        case string name when name.EndsWith("Fortnite"):
                            _roleToEmoteGames.Add(role, "🦙");
                            break;
                        //case "💾 Minecraft":
                        case string name when name.EndsWith("Minecraft"):
                            _roleToEmoteGames.Add(role, "🧱");
                            break;
                        //case "💾 Battlefield": 
                        case string name when name.EndsWith("Battlefield"):
                            _roleToEmoteGames.Add(role, "💥");
                            break;
                        //case "💾 Call of Duty":
                        case string name when name.EndsWith("Call of Duty"):
                        case string name2 when name2.EndsWith("COD"):
                            _roleToEmoteGames.Add(role, "🔫");
                            break;
                        //case "💾 Grand Theft Auto":
                        case string name when name.EndsWith("Grand Theft Auto"):
                        case string name2 when name2.EndsWith("GTA"):
                            _roleToEmoteGames.Add(role, "💰");
                            break;
                        //case "💾 League of Legends":
                        case string name when name.EndsWith("League of Legends"):
                        case string name2 when name2.EndsWith("LoL"):
                            _roleToEmoteGames.Add(role, "🤬");
                            break;
                        //case "💾 World of Warcraft":
                        case string name when name.EndsWith("World of Warcraft"):                        
                            _roleToEmoteGames.Add(role, "🐼");
                            break;
                        //case "🔌 PC":
                        case string name when name.EndsWith("PC"):
                            _roleToEmoteGames.Add(role, "⌨️");
                            break;
                        //case "🔌 Mac":
                        case string name when name.EndsWith("Mac"):
                            _roleToEmoteGames.Add(role, "🍎");
                            break;
                        //case "🔌 Switch":
                        case string name when name.EndsWith("Switch"):
                            _roleToEmoteGames.Add(role, "🎌");
                            break;
                        //case "🔌 PlayStation":
                        case string name when name.EndsWith("PlayStation"):
                        case string name2 when name2.EndsWith("PS5"):
                            _roleToEmoteGames.Add(role, "🎮");
                            break;
                    }
                }                                          
            }            
        }

        private Task CheckAttribution()
        {
            var chrono = new Stopwatch();
            chrono.Start();

            var channelRules = Helper.GetSocketMessageChannel(_client, 846714456788172800); //rôles 
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
                        if (okUser.Id != 493020872303443969) // compte qui met les reaction 
                        {
                            SocketGuildUser subject = _allUsers.First(x => x.Id == okUser.Id);                            
                            if (!subject.Roles.Contains(roleToAssign))
                            {
                                subject.AddRoleAsync(roleToAssign);
                                log.Info($"SPE_{roleToAssign.Name} add for {subject.Username}");
                            }
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
                        if (okUser.Id != 493020872303443969) // compte qui met les reaction 
                        {
                            SocketGuildUser subject = _allUsers.First(x => x.Id == okUser.Id);                            
                            if (!subject.Roles.Contains(roleToAssign))
                            {
                                subject.AddRoleAsync(roleToAssign);
                                log.Info($"GAME_{roleToAssign.Name} add for {subject.Username}");
                            }                            
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

            var channelRules = Helper.GetSocketMessageChannel(_client, 846694705177165864); //rôles
            IReadOnlyCollection<IMessage> iMsg = channelRules.GetMessagesAsync(1).FirstAsync().Result;
            IMessage msg = iMsg.First();

            List<IReadOnlyCollection<IUser>> reactListUsers = msg.GetReactionUsersAsync(msg.Reactions.FirstOrDefault().Key, 1000).ToListAsync().Result;

            foreach (var userLst in reactListUsers)
            {
                var okUserslist = userLst.ToList();

                foreach (var okUser in okUserslist)
                {
                    if (okUser.Id != 493020872303443969) // compte qui met les reaction 
                    {
                        var subject = _allUsers.First(x => x.Id == okUser.Id);                        
                        
                        if (!subject.Roles.Contains(_IRoleRules))
                            await subject.AddRoleAsync(_IRoleRules);
                        
                        log.Info($"CheckRules done for {subject.Username}");
                    }
                }
            }

            chrono.Stop();
            log.Info($"CheckRules done in {chrono.ElapsedMilliseconds}ms");
        }

        public async Task UpdateListUser()
        {
            await _client.DownloadUsersAsync(_client.Guilds); // DL all user

            _allUsers = Helper.GetZderLand(_client).Users.ToList();
            _allUsers.RemoveAll(x => x.IsBot);
        }
        #endregion


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
